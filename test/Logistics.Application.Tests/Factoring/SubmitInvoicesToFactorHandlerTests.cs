using System.Linq.Expressions;
using Logistics.Application.Abstractions.Documents;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Abstractions.Storage;
using Logistics.Application.Modules.Financial.Factoring.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Xunit;
using static Logistics.Application.Tests.Factoring.FactoringTestData;

namespace Logistics.Application.Tests.Factoring;

public class SubmitInvoicesToFactorHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<LoadInvoice, Guid> _invoiceRepo = Substitute.For<ITenantRepository<LoadInvoice, Guid>>();
    private readonly ITenantRepository<FactoringCompany, Guid> _companyRepo = Substitute.For<ITenantRepository<FactoringCompany, Guid>>();
    private readonly ITenantRepository<LoadDocument, Guid> _documentRepo = Substitute.For<ITenantRepository<LoadDocument, Guid>>();
    private readonly ITenantRepository<FactoringBatch, Guid> _batchRepo = Substitute.For<ITenantRepository<FactoringBatch, Guid>>();
    private readonly ITenantRepository<FactoringBatchItem, Guid> _itemRepo = Substitute.For<ITenantRepository<FactoringBatchItem, Guid>>();
    private readonly IBlobStorageService _blobStorage = Substitute.For<IBlobStorageService>();
    private readonly IThreadedEmailSender _emailSender = Substitute.For<IThreadedEmailSender>();
    private readonly IEmailTemplateService _templates = Substitute.For<IEmailTemplateService>();
    private readonly SubmitInvoicesToFactorHandler _sut;

    private readonly FactoringCompany _factor = Company();
    private readonly List<LoadInvoice> _invoices = [];
    private readonly List<LoadDocument> _documents = [];

    public SubmitInvoicesToFactorHandlerTests()
    {
        _tenantUow.Repository<LoadInvoice>().Returns(_invoiceRepo);
        _tenantUow.Repository<FactoringCompany>().Returns(_companyRepo);
        _tenantUow.Repository<LoadDocument>().Returns(_documentRepo);
        _tenantUow.Repository<FactoringBatch>().Returns(_batchRepo);
        _tenantUow.Repository<FactoringBatchItem>().Returns(_itemRepo);
        _tenantUow.GetCurrentTenant().Returns(new Tenant
        {
            Name = "Carrier",
            DatabaseName = "test",
            BillingEmail = "billing@carrier.test",
            CompanyAddress = Address
        });

        _invoiceRepo.GetListAsync(Arg.Any<Expression<Func<LoadInvoice, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(_ => _invoices.ToList());
        _invoiceRepo.Query().Returns(_ => _invoices.BuildMock());
        _documentRepo.GetListAsync(Arg.Any<Expression<Func<LoadDocument, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(_ => _documents.ToList());
        _itemRepo.GetListAsync(Arg.Any<Expression<Func<FactoringBatchItem, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(_ => new List<FactoringBatchItem>());
        _companyRepo.GetByIdAsync(_factor.Id, Arg.Any<CancellationToken>()).Returns(_factor);
        _blobStorage.DownloadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ => new MemoryStream([1, 2, 3]));
        _templates.RenderAsync(Arg.Any<string>(), Arg.Any<FactoringScheduleEmailModel>()).Returns("<p>schedule</p>");
        _emailSender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>())
            .Returns(new ThreadedEmailResult(true, "msg_1"));

        var invoicePdf = Substitute.For<IInvoicePdfService>();
        invoicePdf.GenerateLoadInvoicePdf(Arg.Any<LoadInvoice>(), Arg.Any<Tenant>()).Returns([9]);
        var schedulePdf = Substitute.For<IFactoringSchedulePdfService>();
        schedulePdf.GenerateSchedulePdf(Arg.Any<FactoringBatch>(), Arg.Any<Tenant>()).Returns([8]);

        _sut = new SubmitInvoicesToFactorHandler(_tenantUow, invoicePdf, schedulePdf, _blobStorage, _emailSender,
            _templates, NullLogger<SubmitInvoicesToFactorHandler>.Instance);
    }

    private LoadInvoice AddInvoice(FactoringCompany? factor, long number, bool withPod = true)
    {
        var invoice = Invoice(Customer(factor), number);
        _invoices.Add(invoice);
        if (withPod)
        {
            _documents.Add(LoadDocument.Create("pod.pdf", "pod.pdf", "application/pdf", 3, "path/pod.pdf", "docs",
                DocumentType.ProofOfDelivery, invoice.LoadId, Guid.NewGuid()));
        }

        return invoice;
    }

    private SubmitInvoicesToFactorCommand Command() => new() { InvoiceIds = _invoices.Select(i => i.Id).ToList() };

    #region Validation

    [Fact]
    public async Task Handle_InvoiceWithoutPodOrBol_FailsAndSendsNothing()
    {
        AddInvoice(_factor, 1001);
        AddInvoice(_factor, 1002, withPod: false);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invoice #1002 has no POD or BOL", result.Error);
        Assert.DoesNotContain("#1001", result.Error);
        await _batchRepo.DidNotReceive().AddAsync(Arg.Any<FactoringBatch>(), Arg.Any<CancellationToken>());
        await _emailSender.DidNotReceive().SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InvoiceAlreadySubmitted_Fails()
    {
        AddInvoice(_factor, 1001).FactoringStatus = FactoringStatus.Funded;

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Invoice #1001 is already submitted", result.Error);
    }

    [Fact]
    public async Task Handle_CustomersWithDifferentFactors_Fails()
    {
        AddInvoice(_factor, 1001);
        AddInvoice(Company(), 1002);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("same company", result.Error);
    }

    [Fact]
    public async Task Handle_CustomerNotFactored_Fails()
    {
        AddInvoice(null, 1001);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_PaidInvoice_Fails()
    {
        AddInvoice(_factor, 1001).Status = InvoiceStatus.Paid;

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("only issued, unpaid invoices", result.Error);
    }

    #endregion

    #region Submission

    [Fact]
    public async Task Handle_EligibleInvoices_EmailsScheduleInvoicesAndPaperworkToTheFactor()
    {
        var first = AddInvoice(_factor, 1001);
        var second = AddInvoice(_factor, 1002);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(FactoringStatus.Submitted, first.FactoringStatus);
        Assert.Equal(FactoringStatus.Submitted, second.FactoringStatus);
        await _batchRepo.Received(1).AddAsync(
            Arg.Is<FactoringBatch>(b => b.Items.Count == 2 && b.Total.Amount == 2000m), Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendAsync(
            Arg.Is<ThreadedEmail>(e => e.To == "schedules@acme.test"
                                       && e.ReplyTo == "billing@carrier.test"
                                       && e.Attachments!.Count == 5
                                       && e.Attachments[0].FileName.StartsWith("Schedule-")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailFails_RemovesTheScheduleAndRestoresTheInvoices()
    {
        var invoice = AddInvoice(_factor, 1001);
        invoice.FactoringStatus = FactoringStatus.Rejected;
        _emailSender.SendAsync(Arg.Any<ThreadedEmail>(), Arg.Any<CancellationToken>())
            .Returns(new ThreadedEmailResult(false, null));

        var result = await _sut.Handle(Command(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _batchRepo.Received(1).Delete(Arg.Any<FactoringBatch>());
        Assert.Equal(FactoringStatus.Rejected, invoice.FactoringStatus);
    }

    #endregion
}
