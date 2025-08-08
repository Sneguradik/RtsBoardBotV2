using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Quartz;

namespace RobotWorker;

public class PosterJob(IQuotesStorage quotesStorage, IQuoteRepo quoteRepo, IOrderBookRepo orderBookRepo, ILogger<PosterJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var quotes = quotesStorage.GetAllAsync();
        var quotesToUpdate = new List<Quote>();

        var tickers = orderBookRepo.GetTickers();
        
        logger.LogInformation($"{DateTime.Now:dd-MM-yy hh:mm:ss} - Started processing {tickers.Count()} tickers.");

        var processorCounter = 0;

        foreach (var ticker in tickers)
        {
            var book = orderBookRepo.GetOrderBook(ticker);
            if (book == null)
            {
                logger.LogWarning($"OrderBook for {ticker} was not found.");
                continue;
            }
            
            logger.LogInformation($"Processing {ticker}.");
            
            var quotesByInstrument = quotes.Where(x=>x.Instrument == book.Instrument);
            
            
            var sellQuotes = Enumerable.Zip(book.Asks, quotesByInstrument.Where(x=>x.Direction == DealDirection.Sell),
                (first, second) =>
                {

                    second.Price = first.Price;
                    second.Quantity = first.Quantity;
                    return second;
                } );
            
            var buyQuotes = Enumerable.Zip(book.Bids, quotesByInstrument.Where(x=>x.Direction == DealDirection.Buy),
                (first, second) =>
                {
                    second.Price = first.Price;
                    second.Quantity = first.Quantity;
                    return second;
                } );
            quotesToUpdate.AddRange(sellQuotes);
            quotesToUpdate.AddRange(buyQuotes);
            
            processorCounter++;
        }
        
        await quoteRepo.UpdateQuotesAsync(quotesToUpdate,context.CancellationToken);
        
        logger.LogInformation($"{DateTime.Now:dd-MM-yy hh:mm:ss} - Processed {processorCounter} ticker.");
    }
}