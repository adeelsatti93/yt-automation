using KidsCartoonPipeline.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsCartoonPipeline.Infrastructure.Data.Configurations;

public class DialogueLineConfiguration : IEntityTypeConfiguration<DialogueLine>
{
    public void Configure(EntityTypeBuilder<DialogueLine> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.CharacterName).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Text).HasColumnType("text").IsRequired();
        builder.Property(d => d.Tone).HasMaxLength(50);
        builder.Property(d => d.AudioPath).HasMaxLength(500);
    }
}
