using Logistics.Application.Modules.Integrations.FuelCards.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.FuelCards;

public class AssignFuelCardTransactionTruckHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();

    private readonly ITenantRepository<FuelCardTransaction, Guid> transactionRepo =
        Substitute.For<ITenantRepository<FuelCardTransaction, Guid>>();
    private readonly ITenantRepository<Truck, Guid> truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<FuelCard, Guid> cardRepo =
        Substitute.For<ITenantRepository<FuelCard, Guid>>();
    private readonly ITenantRepository<Expense, Guid> expenseRepo =
        Substitute.For<ITenantRepository<Expense, Guid>>();

    private readonly Truck truck;
    private readonly FuelCardTransaction transaction;
    private readonly AssignFuelCardTransactionTruckHandler sut;

    public AssignFuelCardTransactionTruckHandlerTests()
    {
        truck = new Truck { Number = "T-200", Type = TruckType.FreightTruck };
        transaction = new FuelCardTransaction
        {
            ProviderType = FuelCardProviderType.Demo,
            ExternalTransactionId = "TX-1",
            TransactionDate = DateTime.UtcNow,
            Amount = new Money { Amount = 200, Currency = "USD" },
            ExternalCardId = "CARD-1"
        };

        tenantUow.Repository<FuelCardTransaction>().Returns(transactionRepo);
        tenantUow.Repository<Truck>().Returns(truckRepo);
        tenantUow.Repository<FuelCard>().Returns(cardRepo);
        tenantUow.Repository<Expense>().Returns(expenseRepo);

        transactionRepo.GetByIdAsync(transaction.Id, Arg.Any<CancellationToken>()).Returns(transaction);
        truckRepo.GetByIdAsync(truck.Id, Arg.Any<CancellationToken>()).Returns(truck);
        cardRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<FuelCard, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((FuelCard?)null);

        sut = new AssignFuelCardTransactionTruckHandler(
            tenantUow, NullLogger<AssignFuelCardTransactionTruckHandler>.Instance);
    }

    [Fact]
    public async Task Handle_PendingTransaction_MaterializesExpenseAndMatches()
    {
        var result = await sut.Handle(new AssignFuelCardTransactionTruckCommand
        {
            TransactionId = transaction.Id,
            TruckId = truck.Id
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(FuelCardTransactionStatus.Matched, transaction.Status);
        Assert.Equal(truck.Id, transaction.TruckId);
        Assert.NotNull(transaction.ExpenseId);
        await expenseRepo.Received(1).AddAsync(
            Arg.Is<Expense>(e => e.Status == ExpenseStatus.Paid), Arg.Any<CancellationToken>());
        await cardRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_RememberMapping_CreatesCardMapping()
    {
        var result = await sut.Handle(new AssignFuelCardTransactionTruckCommand
        {
            TransactionId = transaction.Id,
            TruckId = truck.Id,
            RememberMapping = true
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await cardRepo.Received(1).AddAsync(
            Arg.Is<FuelCard>(c => c.ExternalCardId == "CARD-1" && c.TruckId == truck.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlreadyMatched_Fails()
    {
        transaction.Status = FuelCardTransactionStatus.Matched;

        var result = await sut.Handle(new AssignFuelCardTransactionTruckCommand
        {
            TransactionId = transaction.Id,
            TruckId = truck.Id
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await expenseRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_TruckNotFound_Fails()
    {
        var result = await sut.Handle(new AssignFuelCardTransactionTruckCommand
        {
            TransactionId = transaction.Id,
            TruckId = Guid.NewGuid()
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
