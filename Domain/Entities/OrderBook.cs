namespace Domain.Entities;

public class OrderBook
{
    public Instrument Instrument { get; set; } = null!;
    public IEnumerable<MarketLevel> Bids { get; set; } = new List<MarketLevel>();
    public IEnumerable<MarketLevel> Asks { get; set; } = new List<MarketLevel>();

    public void Merge(OrderBook second, int depth)
    {
        if (Instrument != second.Instrument)
        {
            throw new ArgumentException("Cannot merge order books for different instruments");
        }
        
        
        Bids = Bids
            .Concat(second.Bids)
            .GroupBy(b => b.Price)
            .Select(g => new MarketLevel
            {
                Price = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(b => b.Price)
            .Take(depth)
            .ToList();
        
        Asks = Asks
            .Concat(second.Asks)
            .GroupBy(a => a.Price)
            .Select(g => new MarketLevel
            {
                Price = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .OrderBy(a => a.Price)
            .Take(depth)
            .ToList();
    }
}