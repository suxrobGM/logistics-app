using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class FactoringBatchEntityConfiguration : IEntityTypeConfiguration<FactoringBatch>
{
    public void Configure(EntityTypeBuilder<FactoringBatch> builder)
    {
        builder.ToTable("factoring_batches");

        builder.Property(b => b.Number)
            .UseIdentityAlwaysColumn()
            .IsRequired()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        builder.HasIndex(b => b.Number).IsUnique();

        builder.ComplexProperty(b => b.Total, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.Property(b => b.SubmittedToEmail).HasMaxLength(320).IsRequired();
        builder.Property(b => b.Notes).HasMaxLength(2000);
        builder.HasIndex(b => b.Status);

        builder.HasOne(b => b.FactoringCompany)
            .WithMany()
            .HasForeignKey(b => b.FactoringCompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Items)
            .WithOne(i => i.Batch)
            .HasForeignKey(i => i.BatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
