using KidsCartoonPipeline.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsCartoonPipeline.Infrastructure.Data.Configurations;

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Key).HasMaxLength(200).IsRequired();
        builder.HasIndex(s => s.Key).IsUnique();
        builder.Property(s => s.Value).HasColumnType("text");
        builder.Property(s => s.Category).HasMaxLength(100).IsRequired();
        builder.Property(s => s.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(s => s.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
