using Logistics.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Logistics.Infrastructure.Persistence.Conventions;

/// <summary>
/// EF Core convention that automatically applies SnakeCaseEnumConverter to all enum properties,
/// including those inside [ComplexType] value objects.
/// </summary>
public class SnakeCaseEnumConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            ApplyToProperties(entityType.GetProperties());

            foreach (var complexProperty in entityType.GetComplexProperties())
            {
                ApplyToComplexType(complexProperty.ComplexType);
            }
        }
    }

    private static void ApplyToComplexType(IConventionComplexType complexType)
    {
        ApplyToProperties(complexType.GetProperties());

        foreach (var nested in complexType.GetComplexProperties())
        {
            ApplyToComplexType(nested.ComplexType);
        }
    }

    private static void ApplyToProperties(IEnumerable<IConventionProperty> properties)
    {
        foreach (var property in properties)
        {
            var clrType = property.ClrType;
            var underlyingType = Nullable.GetUnderlyingType(clrType) ?? clrType;

            if (underlyingType.IsEnum)
            {
                var converterType = typeof(SnakeCaseEnumConverter<>).MakeGenericType(underlyingType);
                property.SetValueConverter(converterType);
            }
        }
    }
}
