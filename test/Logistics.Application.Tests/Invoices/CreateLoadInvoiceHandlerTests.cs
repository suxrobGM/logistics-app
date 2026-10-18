using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Application.Tests.Operations.Loads;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Invoices;

public class CreateLoadInvoiceHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Load, Guid> _loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly ITenantRepository<LoadInvoice, Guid> _loadInvoiceRepo = Substitute.For<ITenantRepository<LoadInvoice, Guid>>();
    private readonly ITenantRepository<Customer, Guid> _customerRepo = Substitute.For<ITenantRepository<Customer, Guid>>();
    private readonly ITenantRepository<Invoice, Guid> _invoiceRepo = Substitute.For<ITenantRepository<Invoice, Guid>>();
    private readonly CreateLoadInvoiceHandler _sut;

    public CreateLoadInvoiceHandlerTests()
    {
        _tenantUow.Repository<Load>().Returns(_loadRepo);
        _tenantUow.Repository<LoadInvoice>().Returns(_loadInvoiceRepo);
        _tenantUow.Repository<Customer>().Returns(_customerRepo);
        _tenantUow.Repository<Invoice>().Returns(_invoiceRepo);
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());

        // Mirrors InvoiceTaxApplier for a customer without an address: totals come from the lines.
        var taxApplier = Substitute.For<IInvoiceTaxApplier>();
        taxApplier.When(t => t.ApplyAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>()))
            .Do(call => call.Arg<Invoice>().RecalculateTotals());

        _sut = new CreateLoadInvoiceHandler(_tenantUow, taxApplier);
    }

    /// <summary>The invoice once had no lines, so the recalculation billed it at zero.</summary>
    [Fact]
    public async Task Handle_BillsTheLoadChargesAsLines()
    {
        var load = LoadInvoiceChargesTests.NewLoad(linehaul: 2000m);
        load.FuelSurcharge = 300m;
        load.Accessorials.Add(LoadInvoiceChargesTests.Accessorial(load, "Lumper", InvoiceLineItemType.Lumper, 150m));
        _loadRepo.GetByIdAsync(load.Id, Arg.Any<CancellationToken>()).Returns(load);
        _customerRepo.GetByIdAsync(load.Customer.Id, Arg.Any<CancellationToken>()).Returns(load.Customer);
        Invoice? added = null;
        await _invoiceRepo.AddAsync(Arg.Do<Invoice>(i => added = i), Arg.Any<CancellationToken>());

        var result = await _sut.Handle(new CreateLoadInvoiceCommand
        {
            LoadId = load.Id,
            CustomerId = load.Customer.Id
        }, default);

        Assert.True(result.IsSuccess);
        Assert.NotNull(added);
        Assert.Equal(3, added.LineItems.Count);
        Assert.Equal(2450m, added.Total.Amount);
        Assert.Empty(added.Payments);
    }
}
