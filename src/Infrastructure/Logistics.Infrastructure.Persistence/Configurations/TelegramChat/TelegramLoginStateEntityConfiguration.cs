using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TelegramLoginStateEntityConfiguration : IEntityTypeConfiguration<TelegramLoginState>
{
    public void Configure(EntityTypeBuilder<TelegramLoginState> builder)
    {
        builder.ToTable("telegram_login_states");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.State)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(t => t.UserDisplayName)
            .HasMaxLength(256);

        builder.Property(t => t.TenantName)
            .HasMaxLength(256);

        builder.HasIndex(t => t.State).IsUnique();
    }
}
