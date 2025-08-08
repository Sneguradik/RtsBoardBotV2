using Domain.Config;
using Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace RobotWorker;

public class QuotesManager(IServiceProvider serviceProvider, IQuotesStorage quotesStorage, IInstrumentRepo instrumentRepo, IOptions<BotConfig> conf, ILogger<QuotesManager> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var marketDataService = scope.ServiceProvider.GetRequiredService<IOrderBookReceiver>();
        
        var instruments = (await marketDataService.GetInstrumentsAsync(cancellationToken))
            .Where(x=>conf.Value.Tickers.Contains(x.Ticker)).ToList();
        instrumentRepo.AddInstrument(instruments);
        
        logger.LogInformation($"Got {instruments.Count} instruments for {conf.Value.Tickers.Count} tickers.");
        
        var quoteRepo = scope.ServiceProvider.GetRequiredService<IQuoteRepo>();
        
        logger.LogInformation("Started creating quotes.");
        
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var quoteRepo = scope.ServiceProvider.GetRequiredService<IQuoteRepo>();

        var ids = quotesStorage.GetAll().Select(x => x.DocumentId).ToArray();

        await quoteRepo.DeleteQuoteAsync(ids, cancellationToken);
        
        logger.LogInformation($"Deleted {ids.Length} quotes.");
    }
}