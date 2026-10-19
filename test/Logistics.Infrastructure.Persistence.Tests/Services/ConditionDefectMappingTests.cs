using Logistics.Domain.Entities;
using Logistics.Infrastructure.Persistence.Converters;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

public class ConditionDefectMappingTests
{
    [Theory]
    [InlineData(nameof(ConditionDefect.PartCategory))]
    [InlineData(nameof(ConditionDefect.Severity))]
    [InlineData(nameof(ConditionDefect.Zone))]
    [InlineData(nameof(ConditionDefect.DamageType))]
    public void EnumColumns_StoreSnakeCase(string property)
    {
        using var db = new TenantDbContext();

        var converter = db.Model.FindEntityType(typeof(ConditionDefect))!.FindProperty(property)!.GetValueConverter();

        Assert.NotNull(converter);
        Assert.Equal(typeof(SnakeCaseEnumConverter<>), converter.GetType().GetGenericTypeDefinition());
    }

    [Fact]
    public void PhotoDocumentId_IsAForeignKeyToDocumentsThatNullsOnDelete()
    {
        using var db = new TenantDbContext();

        var fk = Assert.Single(db.Model.FindEntityType(typeof(ConditionDefect))!.GetForeignKeys(),
            f => f.Properties.Single().Name == nameof(ConditionDefect.PhotoDocumentId));

        Assert.Equal(typeof(Document), fk.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.SetNull, fk.DeleteBehavior);
    }
}
