using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Logistics.Infrastructure.Persistence.Conventions;

/// <summary>
/// Limits <see cref="Money.Currency"/> to the three letters of an ISO 4217 code on every
/// <see cref="Money"/> complex property, including nested ones.
/// </summary>
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
