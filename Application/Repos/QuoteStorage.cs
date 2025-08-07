using System.Collections.Concurrent;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Repos;

public class QuoteStorage : IQuotesStorage
{
    private ConcurrentBag<Quote> _quotes = new ();
    public void AddAsync(IEnumerable<Quote> quotes, CancellationToken cancellationToken = default)
    {
        foreach (var quote in quotes) 
        {
            _quotes.Add(quote);
        }
    }

    public IEnumerable<Quote> GetAllAsync(CancellationToken cancellationToken = default) => _quotes;
}