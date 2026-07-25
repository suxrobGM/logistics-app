using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AiDispatchPolicyEntityConfiguration : IEntityTypeConfiguration<AiDispatchPolicy>
{
    public void Configure(EntityTypeBuilder<AiDispatchPolicy> builder)
    {
        builder.ToTable("ai_dispatch_policies");

        // Deliberate headroom over the injection cap: a long pass is stored, then clamped at the
        // prompt boundary rather than failing the insert.
        builder.Property(p => p.GeneratedContent)
            .HasMaxLength(DispatchPolicyText.MaxStoredChars);

        builder.Property(p => p.ManualContent)
            .HasMaxLength(DispatchPolicyText.MaxStoredChars);

        builder.Property(p => p.ModelUsed)
            .HasMaxLength(100);

        builder.Property(p => p.IsEnabled)
            .HasDefaultValue(true);

        // Single row per tenant - no index earns its keep.
    }
}
