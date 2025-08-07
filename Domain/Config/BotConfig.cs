namespace Domain.Config;

public class BotConfig
{
    public List<string> Tickers { get; set; } = new();
    public int PostingFrequencyInSeconds { get; set; } 
    public int OrderBookDepth { get; set; }
}