using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

internal sealed class ExpenseEntityConfiguration : IEntityTypeConfiguration<Expense>
{
    // Expense (TPH)
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");

        builder.HasDiscriminator(e => e.Type)
            .HasValue<CompanyExpense>(ExpenseType.Company)
            .HasValue<TruckExpense>(ExpenseType.Truck)
            .HasValue<BodyShopExpense>(ExpenseType.BodyShop);

        builder.Property(e => e.Number)
            .UseIdentityAlwaysColumn()
            .IsRequired();

        builder.HasIndex(e => e.Number)
            .IsUnique();

        builder.ComplexProperty(e => e.Amount, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.Property(e => e.VendorName).HasMaxLength(255);
        builder.Property(e => e.ReceiptBlobPath).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.ApprovedById).HasMaxLength(50);
        builder.Property(e => e.RejectionReason).HasMaxLength(500);

        // Indexes for common queries
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ExpenseDate);
        builder.HasIndex(e => e.Type);
    }

    #region Derived Types Configuration

    public sealed class CompanyExpenseEntityConfiguration : IEntityTypeConfiguration<CompanyExpense>
    {
        public void Configure(EntityTypeBuilder<CompanyExpense> builder)
        {
            // CompanyExpense has no navigation properties, just the Category enum
            builder.Property(e => e.Category).HasConversion<string>();
        }
    }

    public sealed class TruckExpenseEntityConfiguration : IEntityTypeConfiguration<TruckExpense>
    {
        public void Configure(EntityTypeBuilder<TruckExpense> builder)
        {
            builder.Property(e => e.Category).HasConversion<string>();

            builder.HasOne(e => e.Truck)
                .WithMany()
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public sealed class BodyShopExpenseEntityConfiguration : IEntityTypeConfiguration<BodyShopExpense>
    {
        public void Configure(EntityTypeBuilder<BodyShopExpense> builder)
        {
            builder.Property(e => e.VendorAddress).HasMaxLength(500);
            builder.Property(e => e.VendorPhone).HasMaxLength(20);
            builder.Property(e => e.RepairDescription).HasMaxLength(2000);

            builder.HasOne(e => e.Truck)
                .WithMany()
                .HasForeignKey(e => e.TruckId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    #endregion
}
