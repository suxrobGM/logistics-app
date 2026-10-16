using System.Linq.Expressions;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Application.Modules.Integrations.Accounting.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Logistics.Application.Tests.Accounting;

public class ConnectQuickBooksHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IAccountingProviderFactory _factory = Substitute.For<IAccountingProviderFactory>();
    private readonly IAccountingProviderService _provider = Substitute.For<IAccountingProviderService>();
    private readonly IOAuthStateProtector _state = Substitute.For<IOAuthStateProtector>();
    private readonly ITenantRepository<AccountingProviderConfiguration, Guid> _configs =
        Substitute.For<ITenantRepository<AccountingProviderConfiguration, Guid>>();
    private readonly ITenantRepository<QboEntityMapping, Guid> _mappings =
        Substitute.For<ITenantRepository<QboEntityMapping, Guid>>();
    private readonly ConnectQuickBooksHandler _sut;

    private readonly ConnectQuickBooksCommand _command = new() { Code = "code", RealmId = "new-realm", State = "state" };
    private readonly Guid _tenantId = Guid.NewGuid();

    public ConnectQuickBooksHandlerTests()
    {
        _state.TryUnprotect("state").Returns(new OAuthState(_tenantId, Guid.NewGuid()));
        _tenantUow.SetCurrentTenantByIdAsync(_tenantId).Returns(TestTenant.Create());
        _tenantUow.Repository<AccountingProviderConfiguration>().Returns(_configs);
        _tenantUow.Repository<QboEntityMapping>().Returns(_mappings);
        _factory.GetProvider(AccountingProviderType.QuickBooksOnline).Returns(_provider);

        _provider.ExchangeCodeAsync("code", "new-realm", Arg.Any<CancellationToken>()).Returns(new AccountingConnectionResultDto
        {
            AccessToken = "access",
            RefreshToken = "refresh",
            AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(100),
            RealmId = "new-realm",
            CompanyName = "New Co"
        });
        _provider.GetAccountsAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new QboAccountDto { Id = "b1", Name = "Checking", AccountType = "Bank" },
            new QboAccountDto { Id = "e1", Name = "Fuel", AccountType = "Expense" }
        ]);
        _provider.GetItemsAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new QboItemDto { Id = "i9", Name = "Freight", Type = "Service" },
            new QboItemDto { Id = "i1", Name = "Services", Type = "Service" }
        ]);

        _sut = new ConnectQuickBooksHandler(_tenantUow, _factory, _state, NullLogger<ConnectQuickBooksHandler>.Instance);
    }

    [Fact]
    public async Task Handle_DifferentCompany_ClearsOldDefaultsAndMappings()
    {
        var config = OldConnection("old-realm");
        var oldMapping = new QboEntityMapping { LocalId = Guid.NewGuid(), LocalEntityType = QboLocalEntityType.Customer, QboId = "58" };
        _configs.GetAsync(Any<AccountingProviderConfiguration>(), Arg.Any<CancellationToken>()).Returns(config);
        _mappings.GetListAsync(Any<QboEntityMapping>(), Arg.Any<CancellationToken>()).Returns([oldMapping]);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mappings.Received(1).Delete(oldMapping);
        Assert.Equal("new-realm", config.RealmId);
        Assert.Equal("b1", config.DefaultPaymentAccountId);
        Assert.Equal("Bank", config.DefaultPaymentAccountType);
        Assert.Equal("e1", config.DefaultExpenseAccountId);
        Assert.Equal("i1", config.DefaultItemId);
        Assert.Null(config.DepositAccountId);
        Assert.Null(config.PayrollExpenseAccountId);
        Assert.Empty(config.Mappings.Items);
        Assert.False(config.NeedsReconnect);
        Assert.Equal(DateTime.UtcNow.Date, config.SyncFromDate);
    }

    [Fact]
    public async Task Handle_SameCompany_KeepsChoicesAndMappings()
    {
        var config = OldConnection("new-realm");
        _configs.GetAsync(Any<AccountingProviderConfiguration>(), Arg.Any<CancellationToken>()).Returns(config);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mappings.DidNotReceive().Delete(Arg.Any<QboEntityMapping>());
        Assert.Equal("old-bank", config.DefaultPaymentAccountId);
        Assert.Equal("old-deposit", config.DepositAccountId);
        Assert.Single(config.Mappings.Items);
        Assert.False(config.NeedsReconnect);
        Assert.Equal("access", config.AccessToken);
    }

    [Fact]
    public async Task Handle_ExpiredState_FailsWithoutExchangingTheCode()
    {
        _state.TryUnprotect("state").Returns((OAuthState?)null);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _provider.DidNotReceive().ExchangeCodeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CodeExchangeThrows_ReturnsFailure()
    {
        _provider.ExchangeCodeAsync("code", "new-realm", Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("boom"));

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static AccountingProviderConfiguration OldConnection(string realmId)
    {
        var config = new AccountingProviderConfiguration
        {
            ProviderType = AccountingProviderType.QuickBooksOnline,
            RealmId = realmId,
            NeedsReconnect = true,
            DefaultPaymentAccountId = "old-bank",
            DefaultPaymentAccountType = "Bank",
            DefaultExpenseAccountId = "old-expense",
            DefaultItemId = "old-item",
            DepositAccountId = "old-deposit",
            PayrollExpenseAccountId = "old-payroll"
        };
        config.Mappings = new AccountingMappings
        {
            Items = [new LineItemTypeMapping { LineItemType = InvoiceLineItemType.FuelSurcharge, QboId = "old-fsc" }]
        };
        return config;
    }

    private static Expression<Func<T, bool>> Any<T>() => Arg.Any<Expression<Func<T, bool>>>();
}
