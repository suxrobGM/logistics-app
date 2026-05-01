using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Regions;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds 100 freight loads with a realistic type mix (general / reefer / hazmat / intermodal / tank)
/// using region-specific route points, customer names, and container / terminal links.
/// </summary>
internal class LoadSeeder(ILogger<LoadSeeder> logger) : SeederBase(logger)
{
    private readonly DateTime endDate = DateTime.UtcNow.AddDays(-1);
    private readonly DateTime startDate = DateTime.UtcNow.AddMonths(-2);

    public override string Name => nameof(LoadSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 140;

    public override IReadOnlyList<string> DependsOn =>
    [
        nameof(EmployeeSeeder), nameof(TruckSeeder), nameof(CustomerSeeder),
        nameof(TerminalSeeder), nameof(ContainerSeeder)
    ];

    protected override async Task<bool>
        HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken) =>
        await context.TenantUnitOfWork.Repository<Load>().CountAsync(ct: cancellationToken) > 0;

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var employees = context.CreatedEmployees ?? throw new InvalidOperationException("Employees not seeded");
        var trucks = context.CreatedTrucks ?? throw new InvalidOperationException("Trucks not seeded");
        var customers = context.CreatedCustomers ?? throw new InvalidOperationException("Customers not seeded");
        var tenant = context.CurrentTenant ?? throw new InvalidOperationException("Current tenant not set");
        var region = context.Region ?? throw new InvalidOperationException("Region profile not set");
        var terminals = context.CreatedTerminals ?? [];
        var availableContainers = (context.CreatedContainers ?? []).ToList();

        var freightTrucks = trucks.Where(i => i.Type == TruckType.FreightTruck).ToList();
        var containerTrucks = trucks.Where(i => i.Type == TruckType.ContainerTruck).ToList();
        var carHaulerTrucks = trucks.Where(i => i.Type == TruckType.CarHauler).ToList();

        if (freightTrucks.Count == 0 && containerTrucks.Count == 0 && carHaulerTrucks.Count == 0)
        {
            logger.LogWarning("No trucks available for loads");
            LogCompleted();
            return;
        }

        var loadRepository = context.TenantUnitOfWork.Repository<Load>();
        var paymentRepository = context.TenantUnitOfWork.Repository<Payment>();
        var count = 0;

        for (var i = 1; i <= 100; i++)
        {
            var loadType = PickLoadType();
            var requiresContainer = loadType is LoadType.IntermodalContainer or LoadType.TankContainer;
            var requiresCarHauler = loadType is LoadType.Vehicle;

            var truckPool = (requiresContainer, requiresCarHauler) switch
            {
                (true, _) when containerTrucks.Count > 0 => containerTrucks,
                (_, true) when carHaulerTrucks.Count > 0 => carHaulerTrucks,
                _ => freightTrucks
            };

            if (truckPool.Count == 0)
            {
                truckPool = [.. freightTrucks, .. containerTrucks, .. carHaulerTrucks];
            }

            var truck = random.Pick(truckPool);
            var customer = random.Pick(customers);
            var dispatcher = random.Pick(employees.Dispatchers);

            Container? container = null;
            Terminal? originTerminal = null;
            Terminal? destinationTerminal = null;
            Address originAddr;
            Address destAddr;
            GeoPoint originLoc;
            GeoPoint destLoc;

            if (requiresContainer && terminals.Count >= 2)
            {
                originTerminal = random.Pick((IList<Terminal>)terminals);
                var destPool = terminals.Where(t => t != originTerminal).ToList();
                destinationTerminal = random.Pick(destPool);
                originAddr = originTerminal.Address;
                destAddr = destinationTerminal.Address;
                originLoc = new GeoPoint(0, 0);
                destLoc = new GeoPoint(0, 0);

                if (availableContainers.Count > 0)
                {
                    container = random.Pick(availableContainers);
                    availableContainers.Remove(container);
                }
            }
            else
            {
                var origin = random.Pick((IList<RoutePoint>)region.RoutePoints);
                var dest = random.Pick(region.RoutePoints.Where(p => p != origin).ToList());
                originAddr = origin.Address;
                destAddr = dest.Address;
                originLoc = new GeoPoint(origin.Longitude, origin.Latitude);
                destLoc = new GeoPoint(dest.Longitude, dest.Latitude);
            }

            var load = BuildLoad(
                loadType, originAddr, originLoc, destAddr, destLoc,
                truck, dispatcher, customer, container, originTerminal, destinationTerminal,
                region);
            await loadRepository.AddAsync(load, cancellationToken);

            var invoice = load.Invoice!;
            var deliveredAt = load.DeliveredAt ?? DateTime.UtcNow;
            invoice.DueDate = deliveredAt.AddDays(30);
            invoice.SentAt = deliveredAt.AddDays(1);

            if (random.NextDouble() < 0.8)
            {
                var payment = CreatePayment(load, tenant);
                await paymentRepository.AddAsync(payment, cancellationToken);
                invoice.ApplyPayment(payment);
            }

            count++;
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private LoadType PickLoadType()
    {
        var roll = random.NextDouble();
        return roll switch
        {
            < 0.55 => LoadType.GeneralFreight,
            < 0.70 => LoadType.RefrigeratedGoods,
            < 0.83 => LoadType.IntermodalContainer,
            < 0.90 => LoadType.Vehicle,
            < 0.95 => LoadType.TankContainer,
            _ => LoadType.HazardousMaterials
        };
    }

    private Payment CreatePayment(Load load, Tenant tenant)
    {
        var deliveredAt = load.DeliveredAt ?? DateTime.UtcNow;

        return new Payment
        {
            Amount = load.DeliveryCost,
            Status = PaymentStatus.Paid,
            StripePaymentMethodId = null,
            TenantId = tenant.Id,
            Description = $"Payment for Load #{load.Number}",
            BillingAddress = tenant.CompanyAddress,
            ReferenceNumber = $"SEED-{load.Number:D6}",
            RecordedAt = deliveredAt.AddDays(random.Next(5, 25))
        };
    }

    private Load BuildLoad(
        LoadType type,
        Address originAddr,
        GeoPoint originLoc,
        Address destAddr,
        GeoPoint destLoc,
        Truck truck,
        Employee dispatcher,
        Customer customer,
        Container? container,
        Terminal? originTerminal,
        Terminal? destinationTerminal,
        IRegionProfile region)
    {
        var dispatchedAt = random.UtcDate(startDate, endDate);
        var pickedUpAt = dispatchedAt.AddHours(random.Next(1, 12));
        var deliveredAt = pickedUpAt.AddHours(random.Next(4, 48));
        var deliveryCost = random.Next(1_000, 3_000);

        var name = LoadNameBuilder.Build(
            type, customer, originAddr, destAddr, container,
            originTerminal, destinationTerminal, region, random);

        var load = Load.Create(
            name,
            type,
            deliveryCost,
            originAddr,
            originLoc,
            destAddr,
            destLoc,
            customer,
            truck,
            dispatcher);

        load.Source = LoadSource.Manual;
        load.RequestedPickupDate = dispatchedAt.AddHours(-random.Next(2, 24));
        load.RequestedDeliveryDate = deliveredAt.AddHours(random.Next(0, 12));

        if (container is not null)
        {
            load.ContainerId = container.Id;
            load.Container = container;
        }
        if (originTerminal is not null)
        {
            load.OriginTerminalId = originTerminal.Id;
            load.OriginTerminal = originTerminal;
        }
        if (destinationTerminal is not null)
        {
            load.DestinationTerminalId = destinationTerminal.Id;
            load.DestinationTerminal = destinationTerminal;
        }

        load.Dispatch(dispatchedAt);
        load.ConfirmPickup(pickedUpAt);
        load.ConfirmDelivery(deliveredAt);

        return load;
    }
}
