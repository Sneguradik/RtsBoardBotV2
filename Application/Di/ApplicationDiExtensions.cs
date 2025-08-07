using Application.Repos;
using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Di;

public static class ApplicationDiExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IInstrumentRepo, InstrumentRepo>();
        services.AddSingleton<IOrderBookRepo, OrderBookRepo>();
        services.AddSingleton<IQuotesStorage, QuoteStorage>();
        
        return services;
    }
}