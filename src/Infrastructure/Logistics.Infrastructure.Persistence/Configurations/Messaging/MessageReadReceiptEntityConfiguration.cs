using Logistics.Domain.Entities.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class MessageReadReceiptEntityConfiguration : IEntityTypeConfiguration<MessageReadReceipt>
{
    public void Configure(EntityTypeBuilder<MessageReadReceipt> builder)
    {
        builder.ToTable("message_read_receipts");

        builder.HasIndex(r => new { r.MessageId, r.ReadById })
            .IsUnique();

        builder.HasOne(r => r.ReadBy)
            .WithMany()
            .HasForeignKey(r => r.ReadById)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
