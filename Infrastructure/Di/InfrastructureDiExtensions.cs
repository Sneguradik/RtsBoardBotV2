using System.Data;
using System.Data.Odbc;
using Domain.Interfaces;
using Infrastructure.Database;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Di;

public static class InfrastructureDiExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IQuoteRepo, QuoteRepo>();
        services.AddScoped<IOrderBookReceiver, OrderBookReceiver>();

        
        
        services.AddScoped<IDbConnection>(sp =>
        {
            
            var connStr =
                "Driver=FreeTDS;Server=194.247.132.146;Port=1433;Database=OTC_prod;UID=OFFICE\\m.furin;PWD=L4NTvQb9;";
            var conn = new OdbcConnection(connStr);
            conn.Open();
            return conn;
        });
        
        services
            .AddInvestApiClient((_,settings) => settings.AccessToken = "t.HJ28F-SFcrGwLaxCBtiZGz2rkfKfPItA4Xn3HopM534wQmUyVk0NH-zwXvgTtap3KyF5gDUcT4-spanzqvd-Qw");
        
        
        return services;
    }
}