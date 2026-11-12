using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Logistics.Infrastructure.Persistence.Conventions;

/// <summary>Makes every <see cref="Money.Currency"/> column 3 characters long (an ISO 4217 code), including Money inside owned types.</summary>
public class MoneyCurrencyLengthConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var complexProperty in entityType.GetComplexProperties())
            {
                ApplyToComplexType(complexProperty.ComplexType);
            }
        }
    }

    private static void ApplyToComplexType(IConventionComplexType complexType)
    {
        if (complexType.ClrType == typeof(Money))
        {
            complexType.FindProperty(nameof(Money.Currency))?.Builder.HasMaxLength(3);
        }

        foreach (var nested in complexType.GetComplexProperties())
        {
            ApplyToComplexType(nested.ComplexType);
        }
    }
}
