using KidsCartoonPipeline.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsCartoonPipeline.Infrastructure.Data.Configurations;

public class TopicSeedConfiguration : IEntityTypeConfiguration<TopicSeed>
{
    public void Configure(EntityTypeBuilder<TopicSeed> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).HasMaxLength(500).IsRequired();
        builder.Property(t => t.Description).HasColumnType("text");
        builder.Property(t => t.TargetMoral).HasMaxLength(500);
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
