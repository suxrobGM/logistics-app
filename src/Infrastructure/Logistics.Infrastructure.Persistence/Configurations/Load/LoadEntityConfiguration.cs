using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class LoadEntityConfiguration : IEntityTypeConfiguration<Load>
{
    public void Configure(EntityTypeBuilder<Load> builder)
    {
        builder.ToTable("loads");

        builder.ComplexProperty(i => i.DeliveryCost, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.Property(i => i.Number)
            .UseIdentityAlwaysColumn()
            .IsRequired()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasIndex(i => i.Number)
            .IsUnique();

        builder.HasOne(i => i.AssignedDispatcher)
            .WithMany(i => i.DispatchedLoads)
            .HasForeignKey(i => i.AssignedDispatcherId);
        //.OnDelete(DeleteBehavior.SetNull);

        // External load board source tracking
        builder.Property(i => i.ExternalSourceId)
            .HasMaxLength(100);

        builder.Property(i => i.ExternalBrokerReference)
            .HasMaxLength(100);

        // Order-intake & intermodal fields
        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        builder.HasOne(i => i.Container)
            .WithMany()
            .HasForeignKey(i => i.ContainerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.OriginTerminal)
            .WithMany()
            .HasForeignKey(i => i.OriginTerminalId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.DestinationTerminal)
            .WithMany()
            .HasForeignKey(i => i.DestinationTerminalId)
            .OnDelete(DeleteBehavior.SetNull);

        // Computed properties — keep out of EF mapping
        builder.Ignore(i => i.CanConfirmPickUp);
        builder.Ignore(i => i.CanConfirmDelivery);
    }
}
