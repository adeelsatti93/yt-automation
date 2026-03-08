using Hangfire;
using Hangfire.MySql;
using KidsCartoonPipeline.Core.Interfaces.Repositories;
using KidsCartoonPipeline.Core.Interfaces.Services;
using KidsCartoonPipeline.Infrastructure.Data;
using KidsCartoonPipeline.Infrastructure.Repositories;
using KidsCartoonPipeline.Infrastructure.Services.AI;
using KidsCartoonPipeline.Infrastructure.Services.Cache;
using KidsCartoonPipeline.Infrastructure.Services.Pipeline;
using KidsCartoonPipeline.Infrastructure.Services.Settings;
using KidsCartoonPipeline.Infrastructure.Services.Storage;
using KidsCartoonPipeline.Infrastructure.Services.Video;
using KidsCartoonPipeline.Infrastructure.Services.YouTube;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KidsCartoonPipeline.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connStr = config.GetConnectionString("DefaultConnection")!;

        // MySQL + EF Core
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseMySql(connStr, ServerVersion.AutoDetect(connStr),
                x => x.MigrationsAssembly("KidsCartoonPipeline.Infrastructure")));

        // Repositories
        services.AddScoped<IEpisodeRepository, EpisodeRepository>();
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<ITopicRepository, TopicRepository>();
        services.AddScoped<IPipelineJobRepository, PipelineJobRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();

        // Cache
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Settings
        services.AddScoped<ISettingsService, SettingsService>();

        // Storage
        services.AddScoped<IAssetStorageService, LocalAssetStorageService>();

        // AI Services
        services.AddHttpClient<IScriptGenerationService, ClaudeScriptService>();
        services.AddHttpClient<IImageGenerationService, DalleImageService>();
        services.AddHttpClient<IVoiceGenerationService, ElevenLabsVoiceService>();
        services.AddHttpClient<IMusicGenerationService, SunoMusicService>();
        services.AddScoped<ISeoGenerationService, SeoGenerationService>();
        services.AddHttpClient();

        // Video & YouTube
        services.AddScoped<IVideoAssemblyService, FfmpegVideoService>();
        services.AddScoped<IYouTubeService, YouTubeUploadService>();

        // Pipeline
        services.AddScoped<IPipelineOrchestrator, PipelineOrchestrator>();
        services.AddScoped<KidsCartoonPipeline.Infrastructure.Jobs.PipelineTriggerJob>();
        services.AddScoped<KidsCartoonPipeline.Infrastructure.Jobs.AnalyticsSyncJob>();

        // Hangfire
        services.AddHangfire(c => c
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage(new MySqlStorage(connStr, new MySqlStorageOptions
            {
                TablesPrefix = "Hangfire_"
            })));
        services.AddHangfireServer();

        return services;
    }
}
