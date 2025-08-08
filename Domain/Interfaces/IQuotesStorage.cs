using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IQuotesStorage
{
    void AddAsync(IEnumerable<Quote> quotes, CancellationToken cancellationToken = default);
    IEnumerable<Quote> GetAll(CancellationToken cancellationToken = default);

    IEnumerable<Quote> GetQuotesByTickerAndSide(string ticker, DealDirection dealDirection,
        CancellationToken cancellationToken = default);
}