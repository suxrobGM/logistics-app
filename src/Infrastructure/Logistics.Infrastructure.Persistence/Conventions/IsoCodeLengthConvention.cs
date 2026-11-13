using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Logistics.Infrastructure.Persistence.Conventions;

/// <summary>
/// Sizes the ISO code columns inside value objects, nested ones included: <see cref="Money.Currency"/> to 3
/// characters (ISO 4217) and <see cref="Address.Country"/> to 2 (ISO 3166-1 alpha-2).
/// </summary>
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
