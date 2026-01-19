using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds sample expense data including company, truck, and body shop expenses.
/// </summary>
internal class ExpenseSeeder(ILogger<ExpenseSeeder> logger) : SeederBase(logger)
{
    private readonly Random _random = new();
    private readonly DateTime _startDate = DateTime.UtcNow.AddMonths(-3);
    private readonly DateTime _endDate = DateTime.UtcNow.AddDays(-1);

    private static readonly string[] VendorNames =
    [
        "Office Depot", "Staples", "Amazon", "Microsoft", "Google Cloud",
        "State Farm Insurance", "Progressive", "Legal Shield", "Shell",
        "Pilot Flying J", "Love's Travel Stops", "TA Petro", "Goodyear",
        "Bridgestone", "Michelin", "NAPA Auto Parts", "AutoZone",
        "Joe's Body Shop", "Premier Collision Center", "Elite Auto Body"
    ];

    private static readonly CompanyExpenseCategory[] CompanyCategories =
        Enum.GetValues<CompanyExpenseCategory>();

    private static readonly TruckExpenseCategory[] TruckCategories =
        Enum.GetValues<TruckExpenseCategory>();

    public override string Name => nameof(ExpenseSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 160;
    public override IReadOnlyList<string> DependsOn =>
        [nameof(TruckSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Expense>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var expenseRepository = context.TenantUnitOfWork.Repository<Expense>();
        var truckRepository = context.TenantUnitOfWork.Repository<Truck>();

        // Get trucks from context or load from database
        var trucks = context.CreatedTrucks ?? await truckRepository.GetListAsync(ct: cancellationToken);
        if (trucks.Count == 0)
        {
            Logger.LogWarning("No trucks available for expense seeding");
            LogCompleted(0);
            return;
        }
        var count = 0;

        // Seed 20 company expenses
        for (var i = 0; i < 20; i++)
        {
            var expense = CreateCompanyExpense();
            await expenseRepository.AddAsync(expense, cancellationToken);
            count++;
        }

        // Seed 30 truck expenses
        for (var i = 0; i < 30; i++)
        {
            var truck = _random.Pick(trucks);
            var expense = CreateTruckExpense(truck);
            await expenseRepository.AddAsync(expense, cancellationToken);
            count++;
        }

        // Seed 10 body shop expenses
        for (var i = 0; i < 10; i++)
        {
            var truck = _random.Pick(trucks);
            var expense = CreateBodyShopExpense(truck);
            await expenseRepository.AddAsync(expense, cancellationToken);
            count++;
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private CompanyExpense CreateCompanyExpense()
    {
        var expenseDate = _random.UtcDate(_startDate, _endDate);
        var category = _random.Pick(CompanyCategories);
        var amount = category switch
        {
            CompanyExpenseCategory.Office => _random.Next(50, 500),
            CompanyExpenseCategory.Software => _random.Next(100, 2000),
            CompanyExpenseCategory.Insurance => _random.Next(500, 5000),
            CompanyExpenseCategory.Legal => _random.Next(200, 3000),
            CompanyExpenseCategory.Travel => _random.Next(100, 1500),
            _ => _random.Next(50, 1000)
        };

        var expense = new CompanyExpense
        {
            Amount = new Money { Amount = amount, Currency = "USD" },
            VendorName = _random.Pick(VendorNames),
            ExpenseDate = expenseDate,
            ReceiptBlobPath = $"receipts/company/{Guid.NewGuid()}.pdf",
            Notes = $"Company expense for {category}",
            Category = category,
            Status = GetRandomStatus()
        };

        return expense;
    }

    private TruckExpense CreateTruckExpense(Truck truck)
    {
        var expenseDate = _random.UtcDate(_startDate, _endDate);
        var category = _random.Pick(TruckCategories);
        var amount = category switch
        {
            TruckExpenseCategory.Fuel => _random.Next(200, 800),
            TruckExpenseCategory.Maintenance => _random.Next(100, 2000),
            TruckExpenseCategory.Tires => _random.Next(500, 3000),
            TruckExpenseCategory.Registration => _random.Next(100, 500),
            TruckExpenseCategory.Toll => _random.Next(10, 100),
            TruckExpenseCategory.Parking => _random.Next(10, 50),
            _ => _random.Next(50, 500)
        };

        var expense = new TruckExpense
        {
            Amount = new Money { Amount = amount, Currency = "USD" },
            VendorName = _random.Pick(VendorNames),
            ExpenseDate = expenseDate,
            ReceiptBlobPath = $"receipts/truck/{Guid.NewGuid()}.pdf",
            Notes = $"Truck expense for {truck.Number}",
            TruckId = truck.Id,
            Category = category,
            OdometerReading = category == TruckExpenseCategory.Fuel ? _random.Next(100000, 500000) : null,
            Status = GetRandomStatus()
        };

        return expense;
    }

    private BodyShopExpense CreateBodyShopExpense(Truck truck)
    {
        var expenseDate = _random.UtcDate(_startDate, _endDate);
        var completionDate = expenseDate.AddDays(_random.Next(3, 14));

        var repairDescriptions = new[]
        {
            "Front bumper repair and repaint",
            "Side panel dent removal",
            "Rear door replacement",
            "Full body paint touch-up",
            "Fender repair after minor collision",
            "Hood replacement and paint match",
            "Quarter panel restoration"
        };

        var expense = new BodyShopExpense
        {
            Amount = new Money { Amount = _random.Next(1000, 8000), Currency = "USD" },
            VendorName = _random.Pick(["Joe's Body Shop", "Premier Collision Center", "Elite Auto Body", "Quality Auto Repair"]),
            ExpenseDate = expenseDate,
            ReceiptBlobPath = $"receipts/bodyshop/{Guid.NewGuid()}.pdf",
            Notes = $"Body shop repair for truck {truck.Number}",
            TruckId = truck.Id,
            VendorAddress = "123 Auto Repair Lane, Dallas, TX 75001",
            VendorPhone = "(555) 123-4567",
            RepairDescription = _random.Pick(repairDescriptions),
            EstimatedCompletionDate = completionDate,
            ActualCompletionDate = completionDate.AddDays(_random.Next(-2, 3)),
            Status = GetRandomStatus()
        };

        return expense;
    }

    private ExpenseStatus GetRandomStatus()
    {
        var statuses = new[] { ExpenseStatus.Pending, ExpenseStatus.Approved, ExpenseStatus.Approved, ExpenseStatus.Paid };
        return _random.Pick(statuses);
    }
}
