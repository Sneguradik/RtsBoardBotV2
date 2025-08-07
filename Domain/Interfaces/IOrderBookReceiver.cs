using Domain.Entities;

namespace Domain.Interfaces;

public interface IOrderBookReceiver
{
    Task<IEnumerable<Instrument>> GetInstrumentsAsync(CancellationToken cancellationToken = default);
    Task SubscribeAsync(Instrument instrument, CancellationToken cancellationToken = default);
    IAsyncEnumerable<OrderBook> ReceiveOrderBookAsync(CancellationToken cancellationToken = default);
}