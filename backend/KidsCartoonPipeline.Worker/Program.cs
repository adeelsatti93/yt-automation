using Hangfire;
using KidsCartoonPipeline.Infrastructure;
using KidsCartoonPipeline.Infrastructure.Jobs;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Services.AddSerilog();

builder.Services.AddInfrastructure(builder.Configuration);

// Register job classes for DI
builder.Services.AddScoped<PipelineTriggerJob>();
builder.Services.AddScoped<AnalyticsSyncJob>();

var host = builder.Build();

// Register Hangfire recurring jobs using DI-based manager
using (var scope = host.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    jobManager.AddOrUpdate<PipelineTriggerJob>(
        "pipeline-trigger",
        job => job.ExecuteAsync(),
        "*/5 * * * *"); // Every 30 minutes

    jobManager.AddOrUpdate<AnalyticsSyncJob>(
        "analytics-sync",
        job => job.ExecuteAsync(),
        "*/10 * * * *"); // Every 6 hours
}

host.Run();

