using System.Linq.Expressions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Xunit;
using Logistics.Application.Modules.Financial.Payroll.Commands;

namespace Logistics.Application.Tests.Payroll;

public class BatchApprovePayrollHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<PayrollInvoice, Guid> _payrollRepo =
        Substitute.For<ITenantRepository<PayrollInvoice, Guid>>();
    private readonly ICurrentUserService _currentUser = Substitute.For<ICurrentUserService>();
    private readonly Guid _approverId = Guid.NewGuid();
    private readonly BatchApprovePayrollHandler _sut;

    public BatchApprovePayrollHandlerTests()
    {
        _tenantUow.Repository<PayrollInvoice>().Returns(_payrollRepo);
        _currentUser.GetUserId().Returns(_approverId);
        _sut = new BatchApprovePayrollHandler(_tenantUow, _currentUser);
    }

    /// <summary>The event is what notifies the employee, so the batch path must go through Approve().</summary>
    [Fact]
    public async Task Handle_PendingPayrolls_ApprovesAndRaisesTheApprovedEvent()
    {
        var pending = NewPayroll(InvoiceStatus.PendingApproval);
        var draft = NewPayroll(InvoiceStatus.Draft);
        _payrollRepo.GetListAsync(Arg.Any<Expression<Func<PayrollInvoice, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([pending, draft]);

        var result = await _sut.Handle(
            new BatchApprovePayrollCommand { Ids = [pending.Id, draft.Id], Notes = "ok" },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(InvoiceStatus.Approved, pending.Status);
        Assert.Equal(_approverId, pending.ApprovedById);
        Assert.Single(pending.DomainEvents.OfType<PayrollApprovedEvent>());
        Assert.Equal(InvoiceStatus.Draft, draft.Status);
        Assert.Empty(draft.DomainEvents);
    }

    private static PayrollInvoice NewPayroll(InvoiceStatus status)
    {
        var total = new Money { Amount = 1000m, Currency = "USD" };
        return new PayrollInvoice
        {
            EmployeeId = Guid.NewGuid(),
            Subtotal = total,
            TaxTotal = Money.Zero("USD"),
            Total = total,
            Status = status,
            DueDate = DateTime.UtcNow
        };
    }
}
