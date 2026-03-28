using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class DispatchSessionEntityConfiguration : IEntityTypeConfiguration<DispatchSession>
{
    public void Configure(EntityTypeBuilder<DispatchSession> builder)
    {
        builder.ToTable("dispatch_sessions");

        builder.Property(s => s.Number)
            .UseIdentityAlwaysColumn()
            .IsRequired()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.HasIndex(s => s.Number)
            .IsUnique();

        builder.Ignore(s => s.TotalTokensUsed);

        builder.Property(s => s.RequestCost)
            .HasDefaultValue(1);

        builder.Property(s => s.Summary)
            .HasMaxLength(4000);

        builder.Property(s => s.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasMany(s => s.Decisions)
            .WithOne(d => d.Session)
            .HasForeignKey(d => d.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
