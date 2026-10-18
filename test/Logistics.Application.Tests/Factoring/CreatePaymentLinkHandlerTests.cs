using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Financial.PaymentLinks.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Application.Tests.TestKit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Factoring;

public class CreatePaymentLinkHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Invoice, Guid> _invoiceRepo = Substitute.For<ITenantRepository<Invoice, Guid>>();
    private readonly ITenantRepository<PaymentLink, Guid> _linkRepo = Substitute.For<ITenantRepository<PaymentLink, Guid>>();
    private readonly CreatePaymentLinkHandler _sut;

    public CreatePaymentLinkHandlerTests()
    {
        _tenantUow.Repository<Invoice>().Returns(_invoiceRepo);
        _tenantUow.Repository<PaymentLink>().Returns(_linkRepo);
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        _sut = new CreatePaymentLinkHandler(
            _tenantUow,
            Substitute.For<ICurrentUserService>(),
            Options.Create(new CustomerPortalOptions { BaseUrl = "https://portal.test" }),
            NullLogger<CreatePaymentLinkHandler>.Instance);
    }

    [Fact]
    public async Task Handle_FactoredCustomer_FailsWithoutCreatingALink()
    {
        var invoice = FactoringTestData.Invoice(FactoringTestData.Customer(FactoringTestData.Company()));
        _invoiceRepo.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        var result = await _sut.Handle(new CreatePaymentLinkCommand { InvoiceId = invoice.Id, ExpirationDays = 30 }, default);

        Assert.False(result.IsSuccess);
        Assert.Contains("Acme Capital", result.Error);
        await _linkRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Handle_UnfactoredCustomer_CreatesLink()
    {
        var invoice = FactoringTestData.Invoice(FactoringTestData.Customer(factor: null));
        _invoiceRepo.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);

        var result = await _sut.Handle(new CreatePaymentLinkCommand { InvoiceId = invoice.Id, ExpirationDays = 30 }, default);

        Assert.True(result.IsSuccess);
        Assert.StartsWith("https://portal.test/pay/", result.Value!.Url);
    }
}
