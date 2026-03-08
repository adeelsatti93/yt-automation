using KidsCartoonPipeline.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidsCartoonPipeline.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<Scene> Scenes => Set<Scene>();
    public DbSet<DialogueLine> DialogueLines => Set<DialogueLine>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<TopicSeed> TopicSeeds => Set<TopicSeed>();
    public DbSet<PipelineJob> PipelineJobs => Set<PipelineJob>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<YoutubeUpload> YoutubeUploads => Set<YoutubeUpload>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
