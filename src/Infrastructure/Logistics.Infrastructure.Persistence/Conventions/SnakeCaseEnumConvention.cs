using Logistics.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Logistics.Infrastructure.Persistence.Conventions;

/// <summary>
/// EF Core convention that automatically applies SnakeCaseEnumConverter to all enum properties.
/// </summary>
public class SnakeCaseEnumConvention : IModelFinalizingConvention
{
    public void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
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
}
