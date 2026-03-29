using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TelegramChatEntityConfiguration : IEntityTypeConfiguration<TelegramChat>
{
    public void Configure(EntityTypeBuilder<TelegramChat> builder)
    {
        builder.ToTable("telegram_chats");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Username)
            .HasMaxLength(128);

        builder.Property(t => t.FirstName)
            .HasMaxLength(128);

        builder.Property(t => t.GroupTitle)
            .HasMaxLength(256);

        builder.HasIndex(t => t.ChatId).IsUnique();
        builder.HasIndex(t => t.UserId);
    }
}
