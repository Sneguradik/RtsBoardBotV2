using System.Collections.Concurrent;
using Domain.Entities;
using Domain.Enums;
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

    public IEnumerable<Quote> GetAll(CancellationToken cancellationToken = default) => _quotes;
    
    public IEnumerable<Quote> GetQuotesByTickerAndSide(string ticker, DealDirection dealDirection, CancellationToken cancellationToken = default) => 
        _quotes
        .Where(x=>x.Instrument.Ticker==ticker && x.Direction == dealDirection)
        .ToList();
}