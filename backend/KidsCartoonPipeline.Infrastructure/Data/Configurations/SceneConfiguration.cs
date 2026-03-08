using KidsCartoonPipeline.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsCartoonPipeline.Infrastructure.Data.Configurations;

public class SceneConfiguration : IEntityTypeConfiguration<Scene>
{
    public void Configure(EntityTypeBuilder<Scene> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.BackgroundDescription).HasColumnType("text");
        builder.Property(s => s.ActionDescription).HasColumnType("text");
        builder.Property(s => s.ImagePromptUsed).HasColumnType("text");
        builder.HasMany(s => s.DialogueLines)
               .WithOne(d => d.Scene)
               .HasForeignKey(d => d.SceneId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
