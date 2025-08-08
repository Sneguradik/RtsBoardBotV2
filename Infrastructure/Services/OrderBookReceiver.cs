using System.Text.Json;
using Domain.Config;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using Instrument = Domain.Entities.Instrument;
using OrderBook = Domain.Entities.OrderBook;

namespace Infrastructure.Services;

public class OrderBookReceiver(InvestApiClient client, IInstrumentRepo instrumentRepo, IOptions<BotConfig> config) : IOrderBookReceiver
{
    private readonly InstrumentsService.InstrumentsServiceClient _instrumentsServiceClient = client.Instruments;
    private readonly AsyncDuplexStreamingCall<MarketDataRequest,MarketDataResponse> _marketDataStream = client.
        MarketDataStream.MarketDataStream();

    private static double QuotationToDouble(Quotation quotation) => quotation.Units + quotation.Nano * 1e-9;

    private static Instrument ConvertShareToInstrument(Share share) => new()
    {
        Isin = share.Isin,
        Ticker = share.Ticker,
        UId = share.Uid,
        Currency = share.Currency,
    };

    public async Task<IEnumerable<Instrument>> GetInstrumentsAsync(CancellationToken cancellationToken = default)
    {
        var instruments = (await _instrumentsServiceClient.SharesAsync(cancellationToken)).Instruments;
        return instruments.Select(ConvertShareToInstrument);
    }

    public async Task SubscribeAsync(Instrument instrument, CancellationToken cancellationToken = default)
    {
        await _marketDataStream.RequestStream.WriteAsync(new MarketDataRequest()
        {
            SubscribeOrderBookRequest = new SubscribeOrderBookRequest()
            {
                Instruments = { new OrderBookInstrument()
                {
                    
                    InstrumentId = instrument.UId,
                    Depth = config.Value.OrderBookDepth,
                } },
                SubscriptionAction = SubscriptionAction.Subscribe
            }
        }, cancellationToken);
    }

    public async Task SubscribeAsync(IEnumerable<Instrument> instrument, CancellationToken cancellationToken = default)
    {
        var req = new SubscribeOrderBookRequest()
        {
            SubscriptionAction = SubscriptionAction.Subscribe
        };
        req.Instruments.AddRange(instrument.Select(x=>new OrderBookInstrument()
        {
            InstrumentId = x.UId,
            Depth = config.Value.OrderBookDepth,
        }));
        await _marketDataStream.RequestStream.WriteAsync(new MarketDataRequest()
        { SubscribeOrderBookRequest = req }, cancellationToken);
    }

    public async IAsyncEnumerable<OrderBook> ReceiveOrderBookAsync(CancellationToken cancellationToken = default)
    {
        await foreach (var orderBook in _marketDataStream
                           .ResponseStream
                           .ReadAllAsync(cancellationToken: cancellationToken))
        {
            if (orderBook.Orderbook is null) continue;
            var instrument = instrumentRepo.GetInstrumentByUid(orderBook.Orderbook.InstrumentUid);
    
            if (instrument is null) continue;
            

            yield return new OrderBook()
            {
                Instrument = instrument,
                Bids = orderBook
                    .Orderbook
                    .Bids
                    .Take(config.Value.OrderBookDepth)
                    .Select(x=>new MarketLevel() 
                    {
                       Direction = DealDirection.Buy,
                       Price = QuotationToDouble(x.Price),
                       Quantity = x.Quantity
                    }),
                
                Asks = orderBook
                    .Orderbook
                    .Asks
                    .Take(config.Value.OrderBookDepth)
                    .Select(x=>new MarketLevel()
                        {
                           Direction = DealDirection.Sell,
                           Price = QuotationToDouble(x.Price),
                           Quantity = x.Quantity,
                        })
            };

        }
    }
}