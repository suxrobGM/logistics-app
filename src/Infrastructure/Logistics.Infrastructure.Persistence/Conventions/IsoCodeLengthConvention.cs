using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Logistics.Infrastructure.Persistence.Conventions;

/// <summary>Sets the max length of ISO code columns in value objects: 3 for currency codes, 2 for country codes.</summary>
public class IsoCodeLengthConvention : IModelFinalizingConvention
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

        if (complexType.ClrType == typeof(Address))
        {
            complexType.FindProperty(nameof(Address.Country))?.Builder.HasMaxLength(2);
        }

        foreach (var nested in complexType.GetComplexProperties())
        {
            ApplyToComplexType(nested.ComplexType);
        }
    }
}
