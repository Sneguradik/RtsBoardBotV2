using Domain.Entities;

namespace Domain.Interfaces;

public interface IOrderBookRepo
{
    Task InitAsync(CancellationToken token = default);
    void Enqueue(string ticker, OrderBook orderBook);
    OrderBook? GetOrderBook(string ticker);
    IEnumerable<string> GetTickers();
}