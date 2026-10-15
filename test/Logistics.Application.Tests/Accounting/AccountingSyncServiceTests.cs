using System.Linq.Expressions;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Integrations.Accounting.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Logistics.Application.Tests.Accounting;

public class AccountingSyncServiceTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IFeatureService _features = Substitute.For<IFeatureService>();
    private readonly IAccountingConnectionService _connection = Substitute.For<IAccountingConnectionService>();
    private readonly IAccountingProviderService _provider = Substitute.For<IAccountingProviderService>();
    private readonly AccountingSyncService _sut;

    private readonly AccountingProviderConfiguration _config = new()
    {
        ProviderType = AccountingProviderType.QuickBooksOnline,
        RealmId = "realm",
        AccessToken = "access",
        RefreshToken = "refresh"
    };

    private readonly List<QboEntityMapping> _mappings = [];
    private readonly List<Customer> _customers = [];
    private readonly List<LoadInvoice> _invoices = [];

    public AccountingSyncServiceTests()
    {
        var tenant = TestTenant.Create();
        _tenantUow.GetCurrentTenant().Returns(tenant);
        _features.IsFeatureEnabledAsync(tenant.Id, TenantFeature.Accounting).Returns(true);
        _connection.GetProviderAsync(_config, Arg.Any<CancellationToken>()).Returns(_provider);

        var configs = Substitute.For<ITenantRepository<AccountingProviderConfiguration, Guid>>();
        configs.GetAsync(Any<AccountingProviderConfiguration>(), Arg.Any<CancellationToken>()).Returns(_config);
        _tenantUow.Repository<AccountingProviderConfiguration>().Returns(configs);

        var mappings = Substitute.For<ITenantRepository<QboEntityMapping, Guid>>();
        mappings.Query().Returns(_ => _mappings.BuildMock());
        mappings.GetListAsync(Any<QboEntityMapping>(), Arg.Any<CancellationToken>())
            .Returns(ci => _mappings.Where(ci.Arg<Expression<Func<QboEntityMapping, bool>>>().Compile()).ToList());
        mappings.When(m => m.AddAsync(Arg.Any<QboEntityMapping>(), Arg.Any<CancellationToken>()))
            .Do(ci => _mappings.Add(ci.Arg<QboEntityMapping>()));
        _tenantUow.Repository<QboEntityMapping>().Returns(mappings);

        Queryable(_customers);
        Queryable(_invoices);
        Queryable(new List<Expense>());
        Queryable(new List<PayrollInvoice>());
        var lineItems = Substitute.For<ITenantRepository<InvoiceLineItem, Guid>>();
        lineItems.GetListAsync(Any<InvoiceLineItem>(), Arg.Any<CancellationToken>()).Returns([]);
        _tenantUow.Repository<InvoiceLineItem>().Returns(lineItems);

        _provider.UpsertCustomerAsync(Arg.Any<QboCustomerPayload>(), Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>())
            .Returns(ci => new QboUpsertResult { QboId = ci.Arg<QboWriteTarget>().QboId ?? "new", SyncToken = "1" });

        _sut = new AccountingSyncService(_tenantUow, _features, _connection, NullLogger<AccountingSyncService>.Instance);
    }

    #region Gating

    [Fact]
    public async Task SyncAsync_FeatureDisabled_DoesNotTouchTheProvider()
    {
        _features.IsFeatureEnabledAsync(Arg.Any<Guid>(), TenantFeature.Accounting).Returns(false);
        _customers.Add(NewCustomer("Acme"));

        await _sut.SyncAsync(CancellationToken.None);

        await _connection.DidNotReceive().GetProviderAsync(Arg.Any<AccountingProviderConfiguration>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_ConnectionNeedsReconnect_PushesNothing()
    {
        _connection.GetProviderAsync(_config, Arg.Any<CancellationToken>()).Returns((IAccountingProviderService?)null);
        _customers.Add(NewCustomer("Acme"));

        await _sut.SyncAsync(CancellationToken.None);

        await _provider.DidNotReceive().UpsertCustomerAsync(
            Arg.Any<QboCustomerPayload>(), Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>());
        Assert.Null(_config.LastSyncedAt);
    }

    [Fact]
    public async Task SyncAsync_ProviderRejectsToken_ForgetsAccessTokenAndStops()
    {
        _customers.Add(NewCustomer("Acme"));
        _provider.UpsertCustomerAsync(Arg.Any<QboCustomerPayload>(), Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new AccountingReconnectRequiredException("401"));

        await _sut.SyncAsync(CancellationToken.None);

        await _connection.Received(1).RejectAccessTokenAsync(_config, Arg.Any<CancellationToken>());
        Assert.Null(_config.LastSyncedAt);
    }

    #endregion

    #region Selection

    [Fact]
    public async Task SyncAsync_PushesOnlyNewEditedAndFailedCustomers()
    {
        var now = DateTime.UtcNow;
        var fresh = NewCustomer("New");
        var unchanged = NewCustomer("Unchanged");
        var edited = NewCustomer("Edited", updatedAt: now.AddMinutes(-5));
        var failed = NewCustomer("Failed");
        var prospect = NewCustomer("Prospect", CustomerStatus.Prospect);
        _customers.AddRange([fresh, unchanged, edited, failed, prospect]);

        _mappings.Add(Synced(unchanged.Id, lastSyncedAt: now.AddMinutes(-10)));
        _mappings.Add(Synced(edited.Id, lastSyncedAt: now.AddMinutes(-30)));
        _mappings.Add(Failed(failed.Id));

        await _sut.SyncAsync(CancellationToken.None);

        var pushed = _provider.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IAccountingProviderService.UpsertCustomerAsync))
            .Select(c => ((QboCustomerPayload)c.GetArguments()[0]!).Name)
            .ToList();
        Assert.Equal(["Edited", "Failed", "New"], pushed.Order());
        Assert.NotNull(_config.LastSyncedAt);
    }

    [Fact]
    public async Task SyncAsync_NewCustomer_CreatesWithStableRequestIdAndRecordsTheMapping()
    {
        var customer = NewCustomer("Acme");
        _customers.Add(customer);

        await _sut.SyncAsync(CancellationToken.None);

        await _provider.Received(1).UpsertCustomerAsync(
            Arg.Any<QboCustomerPayload>(),
            Arg.Is<QboWriteTarget>(t => t.QboId == null && t.RequestId.Length == 48 && t.RequestId.StartsWith(customer.Id.ToString("N"))),
            Arg.Any<CancellationToken>());
        var mapping = Assert.Single(_mappings);
        Assert.Equal(QboSyncStatus.Synced, mapping.SyncStatus);
        Assert.Equal("new", mapping.QboId);
    }

    [Fact]
    public async Task SyncAsync_PushFails_RecordsTheError()
    {
        _customers.Add(NewCustomer("Acme"));
        _provider.UpsertCustomerAsync(Arg.Any<QboCustomerPayload>(), Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Duplicate Name Exists Error"));

        await _sut.SyncAsync(CancellationToken.None);

        var mapping = Assert.Single(_mappings);
        Assert.Equal(QboSyncStatus.Failed, mapping.SyncStatus);
        Assert.Equal("Duplicate Name Exists Error", mapping.LastError);
    }

    #endregion

    #region Retiring records

    [Fact]
    public async Task SyncAsync_PushedCustomerDeactivated_DeactivatesItInQuickBooks()
    {
        var customer = NewCustomer("Acme", CustomerStatus.Inactive, updatedAt: DateTime.UtcNow.AddMinutes(-1));
        _customers.Add(customer);
        _mappings.Add(Synced(customer.Id, lastSyncedAt: DateTime.UtcNow.AddDays(-1), qboId: "58"));
        _provider.DeactivateCustomerAsync(Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>())
            .Returns(new QboUpsertResult { QboId = "58", SyncToken = "4" });

        await _sut.SyncAsync(CancellationToken.None);

        await _provider.Received(1).DeactivateCustomerAsync(Arg.Is<QboWriteTarget>(t => t.QboId == "58"), Arg.Any<CancellationToken>());
        Assert.Equal(AccountingPayloads.RetiredHash, _mappings[0].LastSyncedHash);
    }

    [Fact]
    public async Task SyncAsync_PushedCustomerDeleted_DeactivatesItInQuickBooks()
    {
        _mappings.Add(Synced(Guid.NewGuid(), lastSyncedAt: DateTime.UtcNow.AddDays(-1), qboId: "77"));
        _provider.DeactivateCustomerAsync(Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>())
            .Returns(new QboUpsertResult { QboId = "77", SyncToken = "2" });

        await _sut.SyncAsync(CancellationToken.None);

        await _provider.Received(1).DeactivateCustomerAsync(Arg.Is<QboWriteTarget>(t => t.QboId == "77"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_PushedInvoiceCancelled_VoidsIt()
    {
        var invoice = NewInvoice(InvoiceStatus.Cancelled, Guid.NewGuid());
        _invoices.Add(invoice);
        _mappings.Add(Synced(invoice.Id, DateTime.UtcNow.AddDays(-1), "130", QboLocalEntityType.Invoice));
        _provider.VoidInvoiceAsync(Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>())
            .Returns(new QboUpsertResult { QboId = "130", SyncToken = "3" });

        await _sut.SyncAsync(CancellationToken.None);

        await _provider.Received(1).VoidInvoiceAsync(Arg.Is<QboWriteTarget>(t => t.QboId == "130"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_InvoiceBeforeSyncFromDate_IsNotPushed()
    {
        _config.SyncFromDate = DateTime.UtcNow.Date;
        var customer = NewCustomer("Acme");
        _customers.Add(customer);
        var invoice = NewInvoice(InvoiceStatus.Issued, customer.Id);
        invoice.CreatedAt = DateTime.UtcNow.AddDays(-30);
        _invoices.Add(invoice);

        await _sut.SyncAsync(CancellationToken.None);

        await _provider.DidNotReceive().UpsertInvoiceAsync(
            Arg.Any<QboInvoicePayload>(), Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncAsync_InvoiceWhoseCustomerIsNotPushed_RecordsAVisibleFailure()
    {
        var invoice = NewInvoice(InvoiceStatus.Issued, Guid.NewGuid());
        _invoices.Add(invoice);

        await _sut.SyncAsync(CancellationToken.None);

        var mapping = Assert.Single(_mappings, m => m.LocalId == invoice.Id);
        Assert.Equal(QboSyncStatus.Failed, mapping.SyncStatus);
        Assert.Contains("customer", mapping.LastError);
    }

    #endregion

    #region Factoring

    [Fact]
    public async Task SyncAsync_FactoredInvoiceSold_DepositsToClearingAndPostsTheFeeFromIt()
    {
        _config.DepositAccountId = "undeposited";
        _config.FactoringClearingAccountId = "clearing";
        _config.FactoringFeeAccountId = "fees";
        var item = AddFundedFactoredInvoice();
        _provider.UpsertPaymentAsync(Arg.Any<QboPaymentPayload>(), Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>())
            .Returns(new QboUpsertResult { QboId = "p1" });
        _provider.UpsertExpenseAsync(Arg.Any<QboExpensePayload>(), Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>())
            .Returns(new QboUpsertResult { QboId = "e1" });

        await _sut.SyncAsync(CancellationToken.None);

        await _provider.Received(1).UpsertPaymentAsync(
            Arg.Is<QboPaymentPayload>(p => p.DepositAccountId == "clearing" && p.Amount == 1000m),
            Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>());
        await _provider.Received(1).UpsertExpenseAsync(
            Arg.Is<QboExpensePayload>(e => e.PaymentAccountId == "clearing" && e.ExpenseAccountId == "fees"
                                           && e.Amount == 30m && e.DocNumber == "FCT-7-1007"),
            Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>());
        Assert.Contains(_mappings, m => m.LocalId == item.Id && m.LocalEntityType == QboLocalEntityType.FactoringFee);
    }

    [Fact]
    public async Task SyncAsync_FactoredInvoiceSoldWithoutClearingAccount_RecordsAVisibleFailure()
    {
        var item = AddFundedFactoredInvoice();

        await _sut.SyncAsync(CancellationToken.None);

        await _provider.DidNotReceive().UpsertPaymentAsync(
            Arg.Any<QboPaymentPayload>(), Arg.Any<QboWriteTarget>(), Arg.Any<CancellationToken>());
        var mapping = Assert.Single(_mappings, m => m.LocalId == item.PaymentId);
        Assert.Equal(QboSyncStatus.Failed, mapping.SyncStatus);
        Assert.Contains("factoring clearing account", mapping.LastError);
    }

    /// <summary>A pushed customer and invoice whose invoice the factor bought: sale payment of 1000, fee 30.</summary>
    private FactoringBatchItem AddFundedFactoredInvoice()
    {
        var customer = NewCustomer("Acme");
        _customers.Add(customer);
        var invoice = NewInvoice(InvoiceStatus.Paid, customer.Id);
        _invoices.Add(invoice);
        _mappings.Add(Synced(customer.Id, DateTime.UtcNow, "c1"));
        _mappings.Add(Synced(invoice.Id, DateTime.UtcNow, "i1", QboLocalEntityType.Invoice));

        var money = new Money { Amount = 1000m, Currency = "USD" };
        var payment = new Payment
        {
            Amount = money,
            Status = PaymentStatus.Paid,
            TenantId = Guid.NewGuid(),
            BillingAddress = new Address { Line1 = "1 Main", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" },
            RecordedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        invoice.Payments.Add(payment);

        var batch = new FactoringBatch
        {
            Number = 7,
            FactoringCompanyId = Guid.NewGuid(),
            Total = money,
            SubmittedToEmail = "f@x.test"
        };
        var item = new FactoringBatchItem
        {
            BatchId = batch.Id,
            Batch = batch,
            InvoiceId = invoice.Id,
            Invoice = invoice,
            Amount = money,
            Status = FactoringStatus.Funded,
            AdvanceAmount = 900m,
            FeeAmount = 30m,
            ReserveAmount = 70m,
            FundedAt = DateTime.UtcNow,
            PaymentId = payment.Id,
            CreatedAt = DateTime.UtcNow
        };
        Queryable(new List<FactoringBatchItem> { item });
        return item;
    }

    #endregion

    private void Queryable<T>(List<T> rows) where T : class, Domain.Core.IEntity<Guid>, Domain.Core.ITenantEntity
    {
        var repo = Substitute.For<ITenantRepository<T, Guid>>();
        repo.Query().Returns(_ => rows.BuildMock());
        _tenantUow.Repository<T>().Returns(repo);
    }

    private static Customer NewCustomer(string name, CustomerStatus status = CustomerStatus.Active, DateTime? updatedAt = null) => new()
    {
        Name = name,
        Status = status,
        CreatedAt = DateTime.UtcNow.AddDays(-2),
        UpdatedAt = updatedAt
    };

    private static LoadInvoice NewInvoice(InvoiceStatus status, Guid customerId) => new()
    {
        LoadId = Guid.NewGuid(),
        CustomerId = customerId,
        Status = status,
        Number = 1007,
        CreatedAt = DateTime.UtcNow.AddDays(-2),
        UpdatedAt = DateTime.UtcNow.AddMinutes(-1),
        Subtotal = Money.Zero("USD"),
        TaxTotal = Money.Zero("USD"),
        Total = Money.Zero("USD")
    };

    private static QboEntityMapping Synced(
        Guid localId, DateTime lastSyncedAt, string qboId = "1", QboLocalEntityType type = QboLocalEntityType.Customer) => new()
    {
        LocalId = localId,
        LocalEntityType = type,
        QboId = qboId,
        QboSyncToken = "0",
        LastSyncedHash = "old",
        LastSyncedAt = lastSyncedAt,
        SyncStatus = QboSyncStatus.Synced
    };

    private static QboEntityMapping Failed(Guid localId) => new()
    {
        LocalId = localId,
        LocalEntityType = QboLocalEntityType.Customer,
        SyncStatus = QboSyncStatus.Failed,
        LastError = "boom"
    };

    private static Expression<Func<T, bool>> Any<T>() => Arg.Any<Expression<Func<T, bool>>>();
}
