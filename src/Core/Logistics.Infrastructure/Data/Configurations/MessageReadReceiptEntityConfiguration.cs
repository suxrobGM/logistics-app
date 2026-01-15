using Logistics.Domain.Entities.Messaging;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

internal sealed class MessageReadReceiptEntityConfiguration : IEntityTypeConfiguration<MessageReadReceipt>
{
    public void Configure(EntityTypeBuilder<MessageReadReceipt> builder)
    {
        builder.ToTable("MessageReadReceipts");

        builder.HasIndex(r => new { r.MessageId, r.ReadById })
            .IsUnique();

        builder.HasOne(r => r.ReadBy)
            .WithMany()
            .HasForeignKey(r => r.ReadById)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
