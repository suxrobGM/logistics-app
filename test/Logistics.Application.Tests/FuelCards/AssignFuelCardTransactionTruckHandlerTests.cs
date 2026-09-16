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
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();

    private readonly ITenantRepository<FuelCardTransaction, Guid> _transactionRepo =
        Substitute.For<ITenantRepository<FuelCardTransaction, Guid>>();
    private readonly ITenantRepository<Truck, Guid> _truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<FuelCard, Guid> _cardRepo =
        Substitute.For<ITenantRepository<FuelCard, Guid>>();
    private readonly ITenantRepository<Expense, Guid> _expenseRepo =
        Substitute.For<ITenantRepository<Expense, Guid>>();

    private readonly Truck _truck;
    private readonly FuelCardTransaction _transaction;
    private readonly AssignFuelCardTransactionTruckHandler _sut;

    public AssignFuelCardTransactionTruckHandlerTests()
    {
        _truck = new Truck { Number = "T-200", Type = TruckType.FreightTruck };
        _transaction = new FuelCardTransaction
        {
            ProviderType = FuelCardProviderType.Demo,
            ExternalTransactionId = "TX-1",
            TransactionDate = DateTime.UtcNow,
            Amount = new Money { Amount = 200, Currency = "USD" },
            ExternalCardId = "CARD-1"
        };

        _tenantUow.Repository<FuelCardTransaction>().Returns(_transactionRepo);
        _tenantUow.Repository<Truck>().Returns(_truckRepo);
        _tenantUow.Repository<FuelCard>().Returns(_cardRepo);
        _tenantUow.Repository<Expense>().Returns(_expenseRepo);

        _transactionRepo.GetByIdAsync(_transaction.Id, Arg.Any<CancellationToken>()).Returns(_transaction);
        _truckRepo.GetByIdAsync(_truck.Id, Arg.Any<CancellationToken>()).Returns(_truck);
        _cardRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<FuelCard, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((FuelCard?)null);

        _sut = new AssignFuelCardTransactionTruckHandler(
            _tenantUow, NullLogger<AssignFuelCardTransactionTruckHandler>.Instance);
    }

    [Fact]
    public async Task Handle_PendingTransaction_MaterializesExpenseAndMatches()
    {
        var result = await _sut.Handle(new AssignFuelCardTransactionTruckCommand
        {
            TransactionId = _transaction.Id,
            TruckId = _truck.Id
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(FuelCardTransactionStatus.Matched, _transaction.Status);
        Assert.Equal(_truck.Id, _transaction.TruckId);
        Assert.NotNull(_transaction.ExpenseId);
        await _expenseRepo.Received(1).AddAsync(
            Arg.Is<Expense>(e => e.Status == ExpenseStatus.Paid), Arg.Any<CancellationToken>());
        await _cardRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_RememberMapping_CreatesCardMapping()
    {
        var result = await _sut.Handle(new AssignFuelCardTransactionTruckCommand
        {
            TransactionId = _transaction.Id,
            TruckId = _truck.Id,
            RememberMapping = true
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _cardRepo.Received(1).AddAsync(
            Arg.Is<FuelCard>(c => c.ExternalCardId == "CARD-1" && c.TruckId == _truck.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AlreadyMatched_Fails()
    {
        _transaction.Status = FuelCardTransactionStatus.Matched;

        var result = await _sut.Handle(new AssignFuelCardTransactionTruckCommand
        {
            TransactionId = _transaction.Id,
            TruckId = _truck.Id
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _expenseRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_TruckNotFound_Fails()
    {
        var result = await _sut.Handle(new AssignFuelCardTransactionTruckCommand
        {
            TransactionId = _transaction.Id,
            TruckId = Guid.NewGuid()
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
