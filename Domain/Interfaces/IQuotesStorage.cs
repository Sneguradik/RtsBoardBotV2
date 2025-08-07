using Domain.Entities;

namespace Domain.Interfaces;

public interface IQuotesStorage
{
    void AddAsync(IEnumerable<Quote> quotes, CancellationToken cancellationToken = default);
    IEnumerable<Quote> GetAllAsync(CancellationToken cancellationToken = default);
}