using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Modules.Platform.DataImport.Constants;
using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.EntityFrameworkCore;
using static Logistics.Application.Modules.Platform.DataImport.Constants.ImportFieldKeys;

namespace Logistics.Application.Modules.Platform.DataImport.Importers;

/// <summary>
///     Trucks match on number. The driver cell links the main driver; the plan's truck limit and the
///     vehicle-transport feature gate apply as they do in the truck form.
/// </summary>
internal sealed class TruckImporter(ITenantUnitOfWork tenantUow, IVehicleTransportGuard vehicleTransportGuard)
    : EntityImporter<TruckImporter.Draft>
{
    private static readonly IReadOnlyDictionary<string, TruckType> TypeAliases = new Dictionary<string, TruckType>
    {
        ["Tractor"] = TruckType.FreightTruck, ["Semi"] = TruckType.FreightTruck, ["Sleeper"] = TruckType.FreightTruck,
        ["Day Cab"] = TruckType.FreightTruck, ["Power Unit"] = TruckType.FreightTruck,
        ["Straight Truck"] = TruckType.BoxTruck, ["Box"] = TruckType.BoxTruck,
        ["Auto Carrier"] = TruckType.CarHauler, ["Car Carrier"] = TruckType.CarHauler,
        ["Container"] = TruckType.ContainerTruck, ["Drayage"] = TruckType.ContainerTruck,
        ["Refrigerated"] = TruckType.Reefer, ["Tank"] = TruckType.Tanker
    };

    private static readonly IReadOnlyDictionary<string, TruckStatus> StatusAliases = new Dictionary<string, TruckStatus>
    {
        ["Active"] = TruckStatus.Available, ["In Service"] = TruckStatus.Available,
        ["OOS"] = TruckStatus.OutOfService, ["Inactive"] = TruckStatus.OutOfService,
        ["Repair"] = TruckStatus.Maintenance, ["Shop"] = TruckStatus.Maintenance
    };

    public override ImportEntityType EntityType => ImportEntityType.Trucks;

    protected override Draft Parse(ImportRow row, ImportOptions options)
    {
        var draft = new Draft(row, row.Get(Number) ?? "");
        draft.ReadRequired(Number, "Truck number");
        draft.Type = draft.ReadEnum(ImportFieldKeys.Type, "Truck type", TypeAliases);
        draft.Status = draft.ReadEnum(Status, "Status", StatusAliases);
        draft.Year = draft.ReadInt(Year, "Year");
        draft.VehicleCapacity = draft.ReadInt(VehicleCapacity, "Vehicle capacity");

        if (draft.Type == TruckType.CarHauler && draft.VehicleCapacity is not > 0)
        {
            draft.Errors.Add("A car hauler needs a vehicle capacity above 0.");
        }

        return draft;
    }

    protected override async Task ResolveAsync(IReadOnlyList<Draft> drafts, ImportOptions options, CancellationToken ct)
    {
        var trucks = await tenantUow.Repository<Truck>().Query().ToListAsync(ct);
        var byNumber = trucks
            .GroupBy(t => t.Number, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var drivers = new EmployeeMatcher(await tenantUow.Repository<Employee>().Query().ToListAsync(ct));
        var claimedDrivers = new Dictionary<Guid, string>();

        foreach (var draft in drafts)
        {
            draft.Existing = byNumber.GetValueOrDefault(draft.Key);
            draft.Exists = draft.Existing is not null;

            if (draft.Type is { } type && type != draft.Existing?.Type &&
                await vehicleTransportGuard.CheckTruckTypeAsync(type) is { IsSuccess: false } typeCheck)
            {
                draft.Errors.Add(typeCheck.Error!);
            }

            if (draft.Row.Get(Driver) is not { } driverCell)
            {
                continue;
            }

            draft.Driver = drivers.Find(driverCell);
            if (draft.Driver is null)
            {
                draft.Errors.Add($"No single employee matches driver '{driverCell}'. Import drivers first.");
                continue;
            }

            if (draft.Driver.Status != EmployeeStatus.Active)
            {
                draft.Errors.Add($"Driver '{driverCell}' is {draft.Driver.Status.GetDescription().ToLowerInvariant()} and cannot be assigned to a truck.");
                continue;
            }

            var otherTruck = trucks.FirstOrDefault(t => t.MainDriverId == draft.Driver.Id && t.Id != draft.Existing?.Id);
            if (otherTruck is not null)
            {
                draft.Errors.Add($"Driver '{driverCell}' already drives truck {otherTruck.Number}.");
            }
            else if (!claimedDrivers.TryAdd(draft.Driver.Id, draft.Key))
            {
                draft.Errors.Add($"Driver '{driverCell}' is also given to truck {claimedDrivers[draft.Driver.Id]} in this file.");
            }
        }

        CheckTruckLimit(drafts, trucks.Count);
    }

    protected override async Task WriteAsync(IReadOnlyList<Draft> drafts, ImportOptions options, CancellationToken ct)
    {
        var repository = tenantUow.Repository<Truck>();
        foreach (var draft in drafts)
        {
            var row = draft.Row;
            var truck = draft.Existing ?? new Truck { Number = draft.Key, Type = TruckType.FreightTruck };

            truck.Type = draft.Type ?? truck.Type;
            truck.Status = draft.Status ?? truck.Status;
            truck.VehicleCapacity = draft.VehicleCapacity ?? truck.VehicleCapacity;
            truck.Vin = row.Get(Vin) ?? truck.Vin;
            truck.Make = row.Get(Make) ?? truck.Make;
            truck.Model = row.Get(Model) ?? truck.Model;
            truck.Year = draft.Year ?? truck.Year;
            truck.LicensePlate = row.Get(LicensePlate) ?? truck.LicensePlate;
            truck.LicensePlateState = row.Get(LicensePlateState) ?? truck.LicensePlateState;
            if (draft.Driver is not null)
            {
                truck.MainDriver = draft.Driver;
                truck.MainDriverId = draft.Driver.Id;
            }

            if (draft.Existing is null)
            {
                await repository.AddAsync(truck, ct);
            }
        }

        await tenantUow.SaveChangesAsync(ct);
    }

    private void CheckTruckLimit(IReadOnlyList<Draft> drafts, int currentCount)
    {
        var plan = tenantUow.GetCurrentTenant().Subscription?.Plan;
        if (plan?.MaxTrucks is not { } maxTrucks)
        {
            return;
        }

        var count = currentCount;
        foreach (var draft in drafts.Where(d => d is { Exists: false, Errors.Count: 0 }))
        {
            if (++count > maxTrucks)
            {
                draft.Errors.Add($"Your {plan.Name} plan allows {maxTrucks} trucks. Upgrade to import more.");
            }
        }
    }

    internal sealed class Draft(ImportRow row, string key) : ImportDraft(row, key)
    {
        public TruckType? Type { get; set; }
        public TruckStatus? Status { get; set; }
        public int? Year { get; set; }
        public int? VehicleCapacity { get; set; }
        public Employee? Driver { get; set; }
        public Truck? Existing { get; set; }
    }
}
