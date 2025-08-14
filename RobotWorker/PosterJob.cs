using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Quartz;

namespace RobotWorker;

public class PosterJob(IQuotesStorage quotesStorage, IQuoteRepo quoteRepo, IOrderBookRepo orderBookRepo, ILogger<PosterJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var tickers = orderBookRepo.GetTickers();
        
        logger.LogInformation($"{DateTime.Now:dd-MM-yy hh:mm:ss} - Started processing {tickers.Count()} tickers.");

        var processorCounter = 0;

        var quotesToUpdate = new List<Quote>();
        var quotesToCreate = new List<Quote>();

        foreach (var ticker in tickers)
        {
            var book = orderBookRepo.GetOrderBook(ticker);
            if (book == null)
            {
                logger.LogWarning($"OrderBook for {ticker} was not found.");
                continue;
            }

            var sellExistingQuotes = quotesStorage.GetQuotesByTickerAndSide(ticker, DealDirection.Sell).ToList();

            if (sellExistingQuotes.Any())
            {
                var sellQuotes = Enumerable.Zip(book.Asks, sellExistingQuotes,
                                (first, second) =>
                                {
                
                                    second.Price = first.Price;
                                    second.Quantity = first.Quantity;
                                    return second;
                                } );
                quotesToUpdate.AddRange(sellQuotes);
            }
            else
            {
                quotesToCreate.AddRange(book.Asks.Select(x=>new Quote()
                {
                    Instrument = book.Instrument,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    Direction = x.Direction,
                    PriceCurrency = book.Instrument.Currency,
                }));
            }
            
            
            
            var buyExistingQuotes = quotesStorage.GetQuotesByTickerAndSide(ticker, DealDirection.Buy).ToList();

            if (buyExistingQuotes.Any())
            {
                var buyQuotes = Enumerable.Zip(book.Bids, buyExistingQuotes,
                                (first, second) =>
                                {
                                    second.Price = first.Price;
                                    second.Quantity = first.Quantity;
                                    return second;
                                } );
                quotesToUpdate.AddRange(buyQuotes);
            }
            else
            {
                quotesToCreate.AddRange(book.Bids.Select(x=>new Quote()
                {
                    Instrument = book.Instrument,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    Direction = x.Direction,
                    PriceCurrency = book.Instrument.Currency,
                }));
            }
            
            
            processorCounter++;
        }
        
        logger.LogInformation($"To update : {string.Join(", ", quotesToUpdate.Select(x=>x.Instrument.Ticker).ToHashSet())}");

        if (quotesToUpdate.Any())
        {
            await quoteRepo.UpdateQuotesAsync(quotesToUpdate,context.CancellationToken);
        }
        
        logger.LogInformation($"To create : {string.Join(", ", quotesToCreate.Select(x=>x.Instrument.Ticker).ToHashSet())}");

        if (quotesToCreate.Any())
        {
            quotesStorage.AddAsync(await quoteRepo.CreateQuotesAsync(quotesToCreate,context.CancellationToken));
        }
        
        logger.LogInformation($"{DateTime.Now:dd-MM-yy hh:mm:ss} - Processed {processorCounter} ticker.");
    }
}