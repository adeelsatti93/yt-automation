using KidsCartoonPipeline.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsCartoonPipeline.Infrastructure.Data.Configurations;

public class PipelineJobConfiguration : IEntityTypeConfiguration<PipelineJob>
{
    public void Configure(EntityTypeBuilder<PipelineJob> builder)
    {
        builder.HasKey(j => j.Id);
        builder.Property(j => j.Stage).HasConversion<string>().HasMaxLength(50);
        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(j => j.ErrorMessage).HasColumnType("text");
        builder.Property(j => j.LogOutput).HasColumnType("longtext");
        builder.Property(j => j.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
