using System.Text.RegularExpressions;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Regions;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Microsoft.AspNetCore.Identity;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Backfills realistic names onto data created by the original single-tenant
/// US seed (which used placeholders like "Gloria Kaufman" and "Freight Load 1").
/// Runs once per tenant, but only acts on the US tenant — the legacy maps are
/// keyed on values that only ever existed there. Idempotent: each rename checks
/// the current value and skips if already updated.
/// </summary>
internal sealed partial class RenameLegacyDemoDataSeeder(ILogger<RenameLegacyDemoDataSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(RenameLegacyDemoDataSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 50; // After DemoTenantsSeeder (40), before any FakeData (100+)
    public override bool IsTenantScoped => true;

    private static readonly Dictionary<string, (string FirstName, string LastName)> UserRenames = new()
    {
        ["owner@test.com"]      = ("Sarah",   "Thompson"),
        ["manager1@test.com"]   = ("Michael", "Carter"),
        ["dispatcher1@test.com"]= ("Jessica", "Reynolds"),
        ["dispatcher2@test.com"]= ("David",   "Chen"),
        ["dispatcher3@test.com"]= ("Brandon", "Mitchell"),
        ["driver1@test.com"]    = ("Robert",  "Hayes"),
        ["driver2@test.com"]    = ("James",   "Wilson"),
        ["driver3@test.com"]    = ("Carlos",  "Ramirez"),
        ["driver4@test.com"]    = ("Tyler",   "Brooks"),
        ["driver5@test.com"]    = ("Marcus",  "Johnson"),
        ["customer1@test.com"]  = ("Linda",   "Murphy"),
        ["customer2@test.com"]  = ("Greg",    "Patterson")
    };

    private static readonly Dictionary<string, string> CustomerRenames = new()
    {
        ["Crowley Maritime"]                  = "Walmart Inc.",
        ["General Steamship Company"]         = "Home Depot",
        ["American Export-Isbrandtsen Lines"] = "Amazon Logistics",
        ["Matson, Inc."]                      = "Target Corporation",
        ["Norton Lilly International"]        = "Kroger Co."
    };

    [GeneratedRegex(@"^(Freight|Car) Load \d+$", RegexOptions.Compiled)]
    private static partial Regex LegacyLoadName();

    [GeneratedRegex(@"^Trip \d+$", RegexOptions.Compiled)]
    private static partial Regex LegacyTripName();

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        // Only act on the US tenant; the legacy data only ever existed there.
        if (context.CurrentTenant?.Name != "us" || context.Region?.Region != Region.Us)
        {
            return;
        }

        LogStarting();

        var random = new Random();
        var userManager = context.UserManager;
        var employeeRepo = context.TenantUnitOfWork.Repository<Employee>();
        var customerRepo = context.TenantUnitOfWork.Repository<Customer>();
        var loadRepo = context.TenantUnitOfWork.Repository<Load>();
        var tripRepo = context.TenantUnitOfWork.Repository<Trip>();

        // 1. Rename Identity users (master DB).
        var renamedUsers = 0;
        foreach (var (email, (first, last)) in UserRenames)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null) continue;
            if (user.FirstName == first && user.LastName == last) continue;

            user.FirstName = first;
            user.LastName = last;
            await userManager.UpdateAsync(user);
            renamedUsers++;
        }

        // 2. Rename employees (tenant DB) — match by email.
        var renamedEmployees = 0;
        foreach (var (email, (first, last)) in UserRenames)
        {
            var employee = await employeeRepo.GetAsync(e => e.Email == email, cancellationToken);
            if (employee is null) continue;
            if (employee.FirstName == first && employee.LastName == last) continue;

            employee.FirstName = first;
            employee.LastName = last;
            employeeRepo.Update(employee);
            renamedEmployees++;
        }

        // 3. Rename customers (tenant DB).
        var renamedCustomers = 0;
        foreach (var (oldName, newName) in CustomerRenames)
        {
            var customer = await customerRepo.GetAsync(c => c.Name == oldName, cancellationToken);
            if (customer is null) continue;

            customer.Name = newName;
            customerRepo.Update(customer);
            renamedCustomers++;
        }

        // 4. Rename loads matching the legacy bland pattern.
        var legacyLoads = await loadRepo.GetListAsync(l => true, cancellationToken);
        var renamedLoads = 0;
        foreach (var load in legacyLoads.Where(l => LegacyLoadName().IsMatch(l.Name)))
        {
            // Legacy car-hauler legs were created as GeneralFreight; upgrade naming where the truck reveals it.
            var effectiveType = load.AssignedTruck?.Type == TruckType.CarHauler ? LoadType.Vehicle : load.Type;
            load.Name = LoadNameBuilder.Build(
                effectiveType,
                load.Customer,
                load.OriginAddress,
                load.DestinationAddress,
                load.Container,
                load.OriginTerminal,
                load.DestinationTerminal,
                context.Region!,
                random);
            loadRepo.Update(load);
            renamedLoads++;
        }

        // 5. Rename trips matching the legacy pattern.
        var legacyTrips = await tripRepo.GetListAsync(t => true, cancellationToken);
        var renamedTrips = 0;
        var corridors = context.Region!.TripCorridorNames;
        var tripIdx = 0;
        foreach (var trip in legacyTrips.Where(t => LegacyTripName().IsMatch(t.Name)))
        {
            trip.Name = $"{corridors[tripIdx % corridors.Count]} #{tripIdx + 1}";
            tripRepo.Update(trip);
            tripIdx++;
            renamedTrips++;
        }

        if (renamedEmployees + renamedCustomers + renamedLoads + renamedTrips > 0)
        {
            await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Renamed {Users} users, {Employees} employees, {Customers} customers, {Loads} loads, {Trips} trips",
            renamedUsers, renamedEmployees, renamedCustomers, renamedLoads, renamedTrips);
        LogCompleted();
    }
}
