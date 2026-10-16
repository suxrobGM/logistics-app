using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds one trailer hooked to each truck, matched to the truck's work (car hauler deck, container
/// chassis, or a van/reefer/flatbed for freight), plus two spare trailers left unhooked in the yard.
/// </summary>
internal class TrailerSeeder(ILogger<TrailerSeeder> logger) : SeederBase(logger)
{
    private static readonly TrailerType[] FreightTypes = [TrailerType.DryVan, TrailerType.Reefer, TrailerType.Flatbed];
    private static readonly string[] Makes = ["Great Dane", "Utility", "Wabash", "Hyundai Translead", "Schmitz Cargobull"];

    public override string Name => nameof(TrailerSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 135;
    public override IReadOnlyList<string> DependsOn => [nameof(TruckSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Trailer>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var trucks = context.CreatedTrucks;
        var region = context.Region ?? throw new InvalidOperationException("Region profile not set");

        if (trucks is null)
        {
            LogUpstreamSkipped();
            return;
        }

        var trailerRepo = context.TenantUnitOfWork.Repository<Trailer>();
        var count = 0;

        foreach (var truck in trucks)
        {
            var trailer = CreateTrailer($"T{501 + count}", TypeFor(truck.Type, count), region);
            await trailerRepo.AddAsync(trailer, cancellationToken);
            truck.HookTrailer(trailer);
            count++;
        }

        for (var spare = 0; spare < 2; spare++)
        {
            var trailer = CreateTrailer($"T{501 + count}", FreightTypes[spare % FreightTypes.Length], region);
            await trailerRepo.AddAsync(trailer, cancellationToken);
            count++;
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private Trailer CreateTrailer(string number, TrailerType type, Regions.IRegionProfile region)
    {
        var make = random.Pick((IList<string>)Makes);
        var plate = region.GeneratePlate();
        return new Trailer
        {
            Number = number,
            Type = type,
            Make = make,
            Year = random.Next(2015, 2025),
            Vin = region.GenerateVin(make),
            LicensePlate = plate.Number,
            LicensePlateState = plate.RegionCode,
            LengthMeters = LengthFor(type, region.Region)
        };
    }

    private static TrailerType TypeFor(TruckType truckType, int index) => truckType switch
    {
        TruckType.CarHauler => TrailerType.CarHauler,
        TruckType.ContainerTruck => TrailerType.Chassis,
        _ => FreightTypes[index % FreightTypes.Length]
    };

    /// <summary>
    /// 53 ft vans in the US, 13.6 m semi-trailers in the EU, 40 ft chassis everywhere.
    /// </summary>
    private static decimal LengthFor(TrailerType type, Region region) => type switch
    {
        TrailerType.Chassis => 12.19m,
        _ when region == Region.EU => 13.60m,
        _ => 16.15m
    };
}
