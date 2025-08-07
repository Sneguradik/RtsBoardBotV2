using Domain.Enums;

namespace Domain.Entities;

public class Quote
{
    public string DocumentId { get; set; } = string.Empty;
    public int RevisionId { get; set; }
    public double Price { get; set; }
    public double Quantity { get; set; }
    public Instrument Instrument { get; set; } = null!;
    public string PriceCurrency { get; set; } = string.Empty;
    
    public DealDirection Direction { get; set; }
}