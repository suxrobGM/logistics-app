using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

/// <summary>
/// Load vehicles are a JSON complex collection on the loads row. Building the query proves the
/// mapping is accepted by EF and Npgsql without a database, and that reading a load selects the
/// vehicles column with it - no second query per load.
/// </summary>
public class LoadVehiclesMappingTests
{
    [Fact]
    public void LoadQuery_SelectsTheVehiclesJsonColumn()
    {
        using var db = new TenantDbContext();

        var sql = db.Set<Load>().Where(l => l.Type == LoadType.Vehicle).ToQueryString();

        Assert.Contains("vehicles", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Model_MapsVehiclesAsComplexCollection()
    {
        using var db = new TenantDbContext();

        var load = db.Model.FindEntityType(typeof(Load))!;

        Assert.NotNull(load.FindComplexProperty(nameof(Load.Vehicles)));
    }
}
