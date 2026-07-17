using Logistics.Application.Abstractions.FuelCards;
using Logistics.Application.Modules.Integrations.FuelCards.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.FuelCards;

public class FuelCardSyncServiceTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IFuelCardProviderFactory providerFactory = Substitute.For<IFuelCardProviderFactory>();
    private readonly IFuelCardProviderService provider = Substitute.For<IFuelCardProviderService>();

    private readonly ITenantRepository<FuelCardProviderConfiguration, Guid> configRepo =
        Substitute.For<ITenantRepository<FuelCardProviderConfiguration, Guid>>();
    private readonly ITenantRepository<FuelCardTransaction, Guid> transactionRepo =
        Substitute.For<ITenantRepository<FuelCardTransaction, Guid>>();
    private readonly ITenantRepository<Truck, Guid> truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<FuelCard, Guid> cardRepo =
        Substitute.For<ITenantRepository<FuelCard, Guid>>();
    private readonly ITenantRepository<Expense, Guid> expenseRepo =
        Substitute.For<ITenantRepository<Expense, Guid>>();

    private readonly FuelCardProviderConfiguration config;
    private readonly Truck truck;
    private readonly FuelCardSyncService sut;

    public FuelCardSyncServiceTests()
    {
        config = new FuelCardProviderConfiguration
        {
            ProviderType = FuelCardProviderType.Demo,
            ApiKey = "demo",
            LastSyncedAt = DateTime.UtcNow.AddDays(-1)
        };
        truck = new Truck { Number = "T-100", Type = TruckType.FreightTruck };

        tenantUow.Repository<FuelCardProviderConfiguration>().Returns(configRepo);
        tenantUow.Repository<FuelCardTransaction>().Returns(transactionRepo);
        tenantUow.Repository<Truck>().Returns(truckRepo);
        tenantUow.Repository<FuelCard>().Returns(cardRepo);
        tenantUow.Repository<Expense>().Returns(expenseRepo);

        configRepo.GetListAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<FuelCardProviderConfiguration, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns([config]);
        truckRepo.GetListAsync(ct: Arg.Any<CancellationToken>()).Returns([truck]);
        cardRepo.GetListAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<FuelCard, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        SetupKnownTransactions([]);

        providerFactory.GetProvider(config).Returns(provider);

        sut = new FuelCardSyncService(tenantUow, providerFactory, NullLogger<FuelCardSyncService>.Instance);
    }

    private void SetupKnownTransactions(List<FuelCardTransaction> existing)
    {
        transactionRepo.Query().Returns(existing.BuildMock());
    }

    private static FuelCardTransactionData CreateData(string externalId, string? unitNumber, string? cardId = null)
    {
        return new FuelCardTransactionData
        {
            ExternalTransactionId = externalId,
            TransactionDate = DateTime.UtcNow.AddHours(-5),
            Amount = 350.25m,
            Quantity = 95m,
            QuantityUnit = VolumeUnit.Gallons,
            PurchaseJurisdiction = new TaxJurisdiction { CountryCode = "US", Region = "TX" },
            UnitNumber = unitNumber,
            ExternalCardId = cardId
        };
    }

    [Fact]
    public async Task Sync_UnitNumberMatchesTruck_MaterializesPaidExpenseAndRemembersCard()
    {
        provider.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([CreateData("TX-1", "T-100", cardId: "CARD-1")]);

        var result = await sut.SyncCurrentTenantAsync();

        Assert.Equal(1, result.Imported);
        Assert.Equal(1, result.Matched);
        Assert.Equal(0, result.Pending);

        await expenseRepo.Received(1).AddAsync(
            Arg.Is<Expense>(e => e is TruckExpense
                && ((TruckExpense)e).TruckId == truck.Id
                && e.Status == ExpenseStatus.Paid
                && ((TruckExpense)e).Category == TruckExpenseCategory.Fuel
                && ((TruckExpense)e).PurchaseJurisdiction!.Region == "TX"),
            Arg.Any<CancellationToken>());
        await cardRepo.Received(1).AddAsync(
            Arg.Is<FuelCard>(c => c.ExternalCardId == "CARD-1" && c.TruckId == truck.Id),
            Arg.Any<CancellationToken>());
        await tenantUow.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sync_UnknownUnitNumber_StagesPendingWithoutExpense()
    {
        provider.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([CreateData("TX-2", "POOL-9")]);

        var result = await sut.SyncCurrentTenantAsync();

        Assert.Equal(1, result.Imported);
        Assert.Equal(0, result.Matched);
        Assert.Equal(1, result.Pending);
        await expenseRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        await transactionRepo.Received(1).AddAsync(
            Arg.Is<FuelCardTransaction>(t => t.Status == FuelCardTransactionStatus.Pending),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sync_KnownExternalId_IsSkipped()
    {
        SetupKnownTransactions([
            new FuelCardTransaction
            {
                ProviderType = FuelCardProviderType.Demo,
                ExternalTransactionId = "TX-3",
                TransactionDate = DateTime.UtcNow,
                Amount = new Money { Amount = 100, Currency = "USD" }
            }
        ]);
        provider.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([CreateData("TX-3", "T-100")]);

        var result = await sut.SyncCurrentTenantAsync();

        Assert.Equal(0, result.Imported);
        await transactionRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Sync_ExistingCardMapping_MatchesWithoutUnitNumber()
    {
        var card = new FuelCard
        {
            ProviderType = FuelCardProviderType.Demo,
            ExternalCardId = "CARD-7",
            TruckId = truck.Id,
            Truck = truck
        };
        cardRepo.GetListAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<FuelCard, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns([card]);
        provider.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([CreateData("TX-4", unitNumber: null, cardId: "CARD-7")]);

        var result = await sut.SyncCurrentTenantAsync();

        Assert.Equal(1, result.Matched);
        await expenseRepo.Received(1).AddAsync(
            Arg.Is<Expense>(e => ((TruckExpense)e).TruckId == truck.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sync_ProviderThrows_RecordsErrorAndContinues()
    {
        provider.GetTransactionsAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<FuelCardTransactionData>>>(_ => throw new HttpRequestException("boom"));

        var result = await sut.SyncCurrentTenantAsync();

        Assert.NotNull(result.Errors);
        Assert.Contains(FuelCardProviderType.Demo, result.Errors.Keys);
        Assert.Equal(0, result.Imported);
    }
}
