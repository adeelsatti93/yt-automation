using KidsCartoonPipeline.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsCartoonPipeline.Infrastructure.Data.Configurations;

public class EpisodeConfiguration : IEntityTypeConfiguration<Episode>
{
    public void Configure(EntityTypeBuilder<Episode> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(500);
        builder.Property(e => e.SeoTitle).HasMaxLength(100);
        builder.Property(e => e.SeoTags).HasColumnType("text");
        builder.Property(e => e.SeoDescription).HasColumnType("text");
        builder.Property(e => e.Summary).HasColumnType("text");
        builder.Property(e => e.ReviewNotes).HasColumnType("text");
        builder.Property(e => e.CurrentStageError).HasColumnType("text");
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.HasMany(e => e.Scenes)
               .WithOne(s => s.Episode)
               .HasForeignKey(s => s.EpisodeId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.PipelineJobs)
               .WithOne(j => j.Episode)
               .HasForeignKey(j => j.EpisodeId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Characters)
               .WithMany(c => c.Episodes)
               .UsingEntity(j => j.ToTable("EpisodeCharacters"));
        builder.HasOne(e => e.TopicSeed)
               .WithOne(t => t.Episode)
               .HasForeignKey<Episode>(e => e.TopicSeedId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
