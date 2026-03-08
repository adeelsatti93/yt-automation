using KidsCartoonPipeline.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsCartoonPipeline.Infrastructure.Data.Configurations;

public class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasColumnType("text").IsRequired();
        builder.Property(c => c.ImagePromptStyle).HasColumnType("text");
        builder.Property(c => c.VoiceId).HasMaxLength(200);
        builder.Property(c => c.VoiceName).HasMaxLength(200);
        builder.Property(c => c.AvatarUrl).HasMaxLength(500);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}
