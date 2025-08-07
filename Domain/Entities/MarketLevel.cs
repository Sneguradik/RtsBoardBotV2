using Domain.Enums;

namespace Domain.Entities;

public class MarketLevel
{
    public double Price { get; set; }
    public double Quantity { get; set; }
    public DealDirection Direction { get; set; }
}
