using Domain.Entities;

namespace Domain.Interfaces;

public interface IQuoteRepo
{
    Task<IEnumerable<Quote>> GetQuotesAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
    Task<Quote?> GetQuoteAsync(string id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Quote>> CreateQuotesAsync(IEnumerable<string> tickers,
        CancellationToken cancellationToken = default);
    
    Task UpdateQuotesAsync(IEnumerable<Quote> quotes, CancellationToken cancellationToken = default);
    
    Task DeleteQuoteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);
}