using System.Collections.Concurrent;
using Domain.Config;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Application.Repos;

public class OrderBookRepo(IOptions<BotConfig> config) :  IOrderBookRepo
{
    private ConcurrentDictionary<string, OrderBook> OrderBooks { get; } = new();
    public Task InitAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public void Enqueue(string ticker, OrderBook orderBook)
    {
        OrderBooks
            .AddOrUpdate(ticker, orderBook, (k, o) => orderBook);
    }

    public OrderBook? GetOrderBook(string ticker) => OrderBooks.GetValueOrDefault(ticker);

    public IEnumerable<string> GetTickers() => OrderBooks.Keys;
}