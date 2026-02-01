using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
///     Seeds general freight loads with invoices.
/// </summary>
internal class LoadSeeder(ILogger<LoadSeeder> logger) : SeederBase(logger)
{
    private readonly DateTime endDate = DateTime.UtcNow.AddDays(-1);
    private readonly DateTime startDate = DateTime.UtcNow.AddMonths(-2);

    public override string Name => nameof(LoadSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 140;

    public override IReadOnlyList<string> DependsOn =>
        [nameof(EmployeeSeeder), nameof(TruckSeeder), nameof(CustomerSeeder)];

    protected override async Task<bool>
        HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken) =>
        await context.TenantUnitOfWork.Repository<Load>().CountAsync(ct: cancellationToken) > 0;

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var employees = context.CreatedEmployees ?? throw new InvalidOperationException("Employees not seeded");
        var trucks = context.CreatedTrucks ?? throw new InvalidOperationException("Trucks not seeded");
        var customers = context.CreatedCustomers ?? throw new InvalidOperationException("Customers not seeded");
        var tenant = context.DefaultTenant ?? throw new InvalidOperationException("Default tenant not set");

        var dryVanTrucks = trucks.Where(i => i.Type == TruckType.FreightTruck).ToList();

        if (dryVanTrucks.Count == 0)
        {
            logger.LogWarning("No freight trucks available for loads");
            LogCompleted();
            return;
        }

        var loadRepository = context.TenantUnitOfWork.Repository<Load>();
        var paymentRepository = context.TenantUnitOfWork.Repository<Payment>();
        var count = 0;

        for (long i = 1; i <= 100; i++)
        {
            var truck = random.Pick(dryVanTrucks);
            var customer = random.Pick(customers);
            var dispatcher = random.Pick(employees.Dispatchers);
            var origin = random.Pick(RoutePoints.Points);
            var dest = random.Pick(RoutePoints.Points.Where(p => p != origin).ToArray());

            var load = BuildLoad(i, origin, dest, LoadType.GeneralFreight, truck, dispatcher, customer);
            await loadRepository.AddAsync(load, cancellationToken);

            // Invoice is created by LoadFactory with line items
            // Dispatch() sets its status to Issued
            var invoice = load.Invoice!;
            var deliveredAt = load.DeliveredAt ?? DateTime.UtcNow;
            invoice.DueDate = deliveredAt.AddDays(30);
            invoice.SentAt = deliveredAt.AddDays(1);

            // Randomly mark some invoices as paid (80% chance for delivered loads)
            var isPaid = random.NextDouble() < 0.8;
            if (isPaid)
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

    private Payment CreatePayment(Load load, Tenant tenant)
    {
        var deliveredAt = load.DeliveredAt ?? DateTime.UtcNow;

        return new Payment
        {
            Amount = load.DeliveryCost,
            Status = PaymentStatus.Paid,
            MethodId = Guid.Empty, // No stored method for seeded payments
            TenantId = tenant.Id,
            Description = $"Payment for Load #{load.Number}",
            BillingAddress = tenant.CompanyAddress,
            ReferenceNumber = $"SEED-{load.Number:D6}",
            RecordedAt = deliveredAt.AddDays(random.Next(5, 25))
        };
    }

    private Load BuildLoad(
        long seq,
        (Address Address, double Longitude, double Latitude) origin,
        (Address Address, double Longitude, double Latitude) dest,
        LoadType type,
        Truck truck,
        Employee dispatcher,
        Customer customer)
    {
        var dispatchedAt = random.UtcDate(startDate, endDate);
        var pickedUpAt = dispatchedAt.AddHours(random.Next(1, 12));
        var deliveredAt = pickedUpAt.AddHours(random.Next(4, 48));
        var deliveryCost = random.Next(1_000, 3_000);
        var originLocation = new GeoPoint(origin.Longitude, origin.Latitude);
        var destLocation = new GeoPoint(dest.Longitude, dest.Latitude);

        var load = Load.Create(
            $"Freight Load {seq}",
            type,
            deliveryCost,
            origin.Address,
            originLocation,
            dest.Address,
            destLocation,
            customer,
            truck,
            dispatcher);

        // Dispatch sets invoice status from Draft to Issued
        load.Dispatch(dispatchedAt);
        load.ConfirmPickup(pickedUpAt);
        load.ConfirmDelivery(deliveredAt);

        return load;
    }
}
