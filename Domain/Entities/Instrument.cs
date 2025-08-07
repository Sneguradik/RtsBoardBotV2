namespace Domain.Entities;

public class Instrument
{
    public string UId { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public string Ticker { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}