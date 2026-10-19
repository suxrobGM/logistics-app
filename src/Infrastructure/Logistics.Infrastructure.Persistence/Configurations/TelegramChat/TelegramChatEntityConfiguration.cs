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

        builder.HasIndex(t => t.ChatId).IsUnique();
        builder.HasIndex(t => t.UserId);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
