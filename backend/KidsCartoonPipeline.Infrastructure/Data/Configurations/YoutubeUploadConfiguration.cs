using KidsCartoonPipeline.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsCartoonPipeline.Infrastructure.Data.Configurations;

public class YoutubeUploadConfiguration : IEntityTypeConfiguration<YoutubeUpload>
{
    public void Configure(EntityTypeBuilder<YoutubeUpload> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.VideoId).HasMaxLength(50);
        builder.Property(u => u.Url).HasMaxLength(200);
        builder.Property(u => u.Status).HasMaxLength(50);
        builder.Property(u => u.EstimatedRevenue).HasColumnType("decimal(10,2)");
        builder.HasOne(u => u.Episode)
               .WithMany()
               .HasForeignKey(u => u.EpisodeId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
