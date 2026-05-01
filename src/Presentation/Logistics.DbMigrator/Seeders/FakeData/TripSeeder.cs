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
/// Seeds car-hauler trips made of multi-leg Vehicle loads, using region-specific corridor names
/// and route points.
/// </summary>
internal class TripSeeder(ILogger<TripSeeder> logger) : SeederBase(logger)
{
    private readonly DateTime startDate = DateTime.UtcNow.AddMonths(-2);
    private readonly DateTime endDate = DateTime.UtcNow.AddDays(-1);

    public override string Name => nameof(TripSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 150;
    public override IReadOnlyList<string> DependsOn =>
        [nameof(TruckSeeder), nameof(CustomerSeeder), nameof(EmployeeSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Trip>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var employees = context.CreatedEmployees ?? throw new InvalidOperationException("Employees not seeded");
        var trucks = context.CreatedTrucks ?? throw new InvalidOperationException("Trucks not seeded");
        var customers = context.CreatedCustomers ?? throw new InvalidOperationException("Customers not seeded");
        var tenant = context.CurrentTenant ?? throw new InvalidOperationException("Current tenant not set");
        var region = context.Region ?? throw new InvalidOperationException("Region profile not set");

        var carHaulerTrucks = trucks.Where(t => t.Type == TruckType.CarHauler).ToList();
        if (carHaulerTrucks.Count == 0)
        {
            logger.LogWarning("No car hauler trucks available for trips");
            LogCompleted(0);
            return;
        }

        var routePoints = region.RoutePoints;
        var corridors = region.TripCorridorNames;
        var tripRepo = context.TenantUnitOfWork.Repository<Trip>();
        var loadRepo = context.TenantUnitOfWork.Repository<Load>();
        var paymentRepo = context.TenantUnitOfWork.Repository<Payment>();
        var count = 0;

        for (var tripIdx = 0; tripIdx < 30; tripIdx++)
        {
            var truck = random.Pick(carHaulerTrucks);
            var dispatcher = random.Pick(employees.Dispatchers);
            var customer = random.Pick(customers);

            var loadsCount = random.Next(2, 5);
            var maxStart = routePoints.Count - (loadsCount + 1);
            if (maxStart < 0)
            {
                continue;
            }
            var startIndex = random.Next(0, maxStart + 1);
            var loads = new List<Load>();

            for (var leg = 0; leg < loadsCount; leg++)
            {
                var origin = routePoints[startIndex + leg];
                var dest = routePoints[startIndex + leg + 1];
                var load = BuildLoad(origin, dest, truck, dispatcher, customer, region);

                loads.Add(load);
                await loadRepo.AddAsync(load, cancellationToken);

                var invoice = load.Invoice!;
                var deliveredAt = load.DeliveredAt ?? DateTime.UtcNow;
                invoice.DueDate = deliveredAt.AddDays(30);
                invoice.SentAt = deliveredAt.AddDays(1);

                if (random.NextDouble() < 0.8)
                {
                    var payment = CreatePayment(load, tenant);
                    await paymentRepo.AddAsync(payment, cancellationToken);
                    invoice.ApplyPayment(payment);
                }
            }

            var tripName = $"{corridors[tripIdx % corridors.Count]} #{tripIdx + 1}";
            var trip = Trip.Create(tripName, truck, loads);
            trip.Dispatch();

            foreach (var stop in trip.Stops)
            {
                trip.MarkStopArrived(stop.Id);
            }

            await tripRepo.AddAsync(trip, cancellationToken);
            count++;
            logger.LogInformation("Created Trip {Trip} with {LoadCount} loads", trip.Name, loadsCount);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
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
        RoutePoint origin,
        RoutePoint dest,
        Truck truck,
        Employee dispatcher,
        Customer customer,
        IRegionProfile region)
    {
        var dispatchedAt = random.UtcDate(startDate, endDate);
        var pickedUpAt = dispatchedAt.AddHours(random.Next(1, 12));
        var deliveredAt = pickedUpAt.AddHours(random.Next(4, 48));
        var deliveryCost = random.Next(1_000, 3_000);
        var originLocation = new GeoPoint(origin.Longitude, origin.Latitude);
        var destLocation = new GeoPoint(dest.Longitude, dest.Latitude);

        var name = LoadNameBuilder.Build(
            LoadType.Vehicle, customer, origin.Address, dest.Address, container: null,
            originTerminal: null, destinationTerminal: null, region, random,
            vehicleCount: random.Next(2, truck.VehicleCapacity == 0 ? 5 : truck.VehicleCapacity + 1));

        var load = Load.Create(
            name,
            LoadType.Vehicle,
            deliveryCost,
            origin.Address,
            originLocation,
            dest.Address,
            destLocation,
            customer,
            truck,
            dispatcher);

        load.Source = LoadSource.Manual;
        load.Dispatch(dispatchedAt);
        load.ConfirmPickup(pickedUpAt);
        load.ConfirmDelivery(deliveredAt);

        return load;
    }
}
