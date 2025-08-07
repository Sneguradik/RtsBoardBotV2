using Domain.Config;
using Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace RobotWorker;

public class MarketDataWorker(IServiceProvider serviceProvider, IInstrumentRepo instrumentRepo, IOrderBookRepo orderBookRepo, ILogger<MarketDataWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var marketDataService = scope.ServiceProvider.GetRequiredService<IOrderBookReceiver>();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var instrument in instrumentRepo.GetInstruments()) await marketDataService.SubscribeAsync(instrument, stoppingToken);
                
                logger.LogInformation("Successfully subscribed to orderbooks.");
                logger.LogInformation("Starting orderbook processing...");
                        
                await foreach(var orderbook in marketDataService.ReceiveOrderBookAsync(stoppingToken))
                {
                    orderBookRepo.Enqueue(orderbook.Instrument.Ticker, orderbook);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                await Task.Delay(3000, stoppingToken);
            }
        }
        
        
    }
}