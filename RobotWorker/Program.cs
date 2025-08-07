using Application.Di;
using Domain.Config;
using Infrastructure.Di;
using Quartz;
using RobotWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices();

builder.Services.Configure<BotConfig>(builder.Configuration.GetSection("BotConfig"));

builder.Services
    .AddHostedService<QuotesManager>()
    .AddHostedService<MarketDataWorker>()
    .AddQuartzHostedService();

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey(nameof(PosterJob));

    q.AddJob<PosterJob>(opts => opts
        .WithIdentity(jobKey)
        .WithDescription("Posts quotes or data periodically"));
            
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity(nameof(PosterJob))
        .WithSimpleSchedule(x => x
            .WithInterval(TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("BotConfig:PostingFrequencyInSeconds")))
            .RepeatForever()));
});

var host = builder.Build();
host.Run();