using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.Geocoding;
using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Modules.Platform.DataImport.Constants;
using Logistics.Application.Modules.Platform.DataImport.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.EntityFrameworkCore;
using static Logistics.Application.Modules.Platform.DataImport.Constants.ImportFieldKeys;

namespace Logistics.Application.Modules.Platform.DataImport.Importers;

/// <summary>
///     Loads match on the source system's reference, kept in <see cref="Load.ExternalSourceId"/> with
///     <see cref="LoadSource.Import"/>. Customers and trucks must already exist. Addresses are geocoded
///     only when the import commits, so a validation run makes no geocoding calls.
/// </summary>
internal sealed class LoadImporter(
    ITenantUnitOfWork tenantUow,
    IGeocodingService geocodingService,
    IVehicleTransportGuard vehicleTransportGuard,
    ICurrentUserService currentUserService) : EntityImporter<LoadImporter.Draft>
{
    private static readonly IReadOnlyDictionary<string, LoadStatus> StatusAliases = new Dictionary<string, LoadStatus>
    {
        ["Open"] = LoadStatus.Draft, ["Booked"] = LoadStatus.Draft, ["Available"] = LoadStatus.Draft,
        ["Planned"] = LoadStatus.Draft, ["Pending"] = LoadStatus.Draft, ["New"] = LoadStatus.Draft,
        ["Assigned"] = LoadStatus.Dispatched, ["Covered"] = LoadStatus.Dispatched,
        ["In Transit"] = LoadStatus.PickedUp, ["Loaded"] = LoadStatus.PickedUp, ["Picked"] = LoadStatus.PickedUp,
        ["Completed"] = LoadStatus.Delivered, ["Complete"] = LoadStatus.Delivered, ["Invoiced"] = LoadStatus.Delivered,
        ["Paid"] = LoadStatus.Delivered, ["Closed"] = LoadStatus.Delivered, ["Billed"] = LoadStatus.Delivered,
        ["Canceled"] = LoadStatus.Cancelled, ["TONU"] = LoadStatus.Cancelled, ["Void"] = LoadStatus.Cancelled
    };

    private static readonly IReadOnlyDictionary<string, LoadType> TypeAliases = new Dictionary<string, LoadType>
    {
        ["Dry"] = LoadType.GeneralFreight, ["Dry Van"] = LoadType.GeneralFreight, ["FTL"] = LoadType.GeneralFreight,
        ["LTL"] = LoadType.GeneralFreight, ["Reefer"] = LoadType.RefrigeratedGoods,
        ["Hazmat"] = LoadType.HazardousMaterials, ["Oversize"] = LoadType.OversizeHeavy,
        ["Tanker"] = LoadType.Liquid, ["Auto"] = LoadType.Vehicle, ["Car"] = LoadType.Vehicle,
        ["Container"] = LoadType.IntermodalContainer, ["Drayage"] = LoadType.IntermodalContainer
    };

    public override ImportEntityType EntityType => ImportEntityType.Loads;

    protected override Draft Parse(ImportRow row, ImportOptions options)
    {
        var draft = new Draft(row, row.Get(Reference) ?? "");
        draft.ReadRequired(Reference, "Load reference");
        draft.ReadRequired(ImportFieldKeys.Customer, "Customer");

        draft.Rate = draft.ReadDecimal(Rate, "Rate");
        if (row.Get(Rate) is null)
        {
            draft.Errors.Add("Rate is required.");
        }
        else if (draft.Rate < 0)
        {
            draft.Errors.Add("Rate cannot be negative.");
        }

        draft.Status = draft.ReadEnum(Status, "Status", StatusAliases) ?? LoadStatus.Draft;
        draft.Type = draft.ReadEnum(ImportFieldKeys.Type, "Load type", TypeAliases) ?? LoadType.GeneralFreight;
        draft.PickupDate = draft.ReadDate(PickupDate, "Pickup date", options);
        draft.DeliveryDate = draft.ReadDate(DeliveryDate, "Delivery date", options);
        draft.Origin = draft.ReadAddress(PickupPrefix, "Pickup address", options, required: true);
        draft.Destination = draft.ReadAddress(DeliveryPrefix, "Delivery address", options, required: true);
        return draft;
    }

    protected override async Task ResolveAsync(IReadOnlyList<Draft> drafts, ImportOptions options, CancellationToken ct)
    {
        var dispatcher = currentUserService.GetUserId() is { } userId
            ? await tenantUow.Repository<Employee>().GetByIdAsync(userId, ct)
            : null;

        var references = drafts.Select(d => d.Key).ToArray();
        var existing = await tenantUow.Repository<Load>().Query()
            .Where(l => l.Source == LoadSource.Import && l.ExternalSourceId != null && references.Contains(l.ExternalSourceId))
            .ToListAsync(ct);
        var customers = await tenantUow.Repository<Customer>().Query().ToListAsync(ct);
        var trucks = await tenantUow.Repository<Truck>().Query().ToListAsync(ct);
        var drivers = new EmployeeMatcher(await tenantUow.Repository<Employee>().Query().ToListAsync(ct));

        if (drafts.Any(d => d.Type == LoadType.Vehicle) &&
            await vehicleTransportGuard.CheckLoadTypeAsync(LoadType.Vehicle) is { IsSuccess: false } typeCheck)
        {
            foreach (var draft in drafts.Where(d => d.Type == LoadType.Vehicle))
            {
                draft.Errors.Add(typeCheck.Error!);
            }
        }

        foreach (var draft in drafts)
        {
            draft.Existing = existing.FirstOrDefault(l => l.ExternalSourceId == draft.Key);
            draft.Exists = draft.Existing is not null;
            draft.Dispatcher = dispatcher;
            if (dispatcher is null)
            {
                draft.Errors.Add("Loads are imported with you as dispatcher, and your user is not an employee here.");
            }

            var customerName = draft.Row.Get(ImportFieldKeys.Customer)!;
            draft.Customer = customers.FirstOrDefault(c => c.Name.Equals(customerName, StringComparison.OrdinalIgnoreCase));
            if (draft.Customer is null)
            {
                draft.Errors.Add($"Unknown customer '{customerName}'. Import customers first.");
            }

            ResolveTruck(draft, trucks, drivers);
        }
    }

    protected override async Task WriteAsync(IReadOnlyList<Draft> drafts, ImportOptions options, CancellationToken ct)
    {
        var geocoded = new Dictionary<Address, GeoPoint?>();
        foreach (var draft in drafts)
        {
            if (draft.Existing is { } load)
            {
                load.AssignedTruck = draft.Truck ?? load.AssignedTruck;
                load.AssignedTruckId = draft.Truck?.Id ?? load.AssignedTruckId;
                load.RequestedPickupDate = draft.PickupDate ?? load.RequestedPickupDate;
                load.RequestedDeliveryDate = draft.DeliveryDate ?? load.RequestedDeliveryDate;
                load.Notes = draft.Row.Get(Notes) ?? load.Notes;
                if (draft.Row.Get(Status) is not null)
                {
                    load.RestoreImportedStatus(draft.Status, draft.PickupDate, draft.DeliveryDate);
                }

                continue;
            }

            var origin = await GeocodeAsync(draft.Origin!, geocoded, ct);
            var destination = await GeocodeAsync(draft.Destination!, geocoded, ct);
            if (origin is null || destination is null)
            {
                draft.Errors.Add($"Could not locate the {(origin is null ? "pickup" : "delivery")} address on the map.");
                continue;
            }

            var created = LoadFactory.Create(draft.Key, draft.Type, draft.Rate!.Value, draft.Origin!, origin,
                draft.Destination!, destination, draft.Customer!, draft.Truck, draft.Dispatcher!,
                currency: options.Currency);

            // History must not push "new load" and "assigned to you" notifications to drivers.
            created.DomainEvents.Clear();
            created.Source = LoadSource.Import;
            created.ExternalSourceId = draft.Key;
            created.RequestedPickupDate = draft.PickupDate;
            created.RequestedDeliveryDate = draft.DeliveryDate;
            created.Notes = draft.Row.Get(Notes);
            created.RestoreImportedStatus(draft.Status, draft.PickupDate, draft.DeliveryDate);
            await tenantUow.Repository<Load>().AddAsync(created, ct);
        }

        await tenantUow.SaveChangesAsync(ct);
    }

    /// <summary>
    ///     The truck cell wins; with only a driver, the load goes to the truck that driver drives.
    /// </summary>
    private static void ResolveTruck(Draft draft, IReadOnlyList<Truck> trucks, EmployeeMatcher drivers)
    {
        if (draft.Row.Get(ImportFieldKeys.Truck) is { } truckNumber)
        {
            draft.Truck = trucks.FirstOrDefault(t => t.Number.Equals(truckNumber, StringComparison.OrdinalIgnoreCase));
            if (draft.Truck is null)
            {
                draft.Errors.Add($"Unknown truck '{truckNumber}'. Import trucks first.");
            }

            return;
        }

        if (draft.Row.Get(Driver) is not { } driverCell)
        {
            return;
        }

        var driver = drivers.Find(driverCell);
        draft.Truck = driver is null ? null : trucks.FirstOrDefault(t => t.IsDrivenBy(driver.Id));
        if (draft.Truck is null)
        {
            draft.Errors.Add($"No truck is driven by '{driverCell}'. Add a truck column or assign the driver to a truck.");
        }
    }

    private async Task<GeoPoint?> GeocodeAsync(Address address, Dictionary<Address, GeoPoint?> cache, CancellationToken ct)
    {
        if (cache.TryGetValue(address, out var cached))
        {
            return cached;
        }

        var result = await geocodingService.GeocodeAddressAsync(
            address.Line1, address.City, address.State, address.ZipCode, address.Country, ct);
        var point = result.IsSuccess ? result.Value : null;
        cache[address] = point;
        return point;
    }

    internal sealed class Draft(ImportRow row, string key) : ImportDraft(row, key)
    {
        public decimal? Rate { get; set; }
        public LoadStatus Status { get; set; }
        public LoadType Type { get; set; }
        public DateTime? PickupDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public Address? Origin { get; set; }
        public Address? Destination { get; set; }
        public Customer? Customer { get; set; }
        public Truck? Truck { get; set; }
        public Employee? Dispatcher { get; set; }
        public Load? Existing { get; set; }
    }
}
