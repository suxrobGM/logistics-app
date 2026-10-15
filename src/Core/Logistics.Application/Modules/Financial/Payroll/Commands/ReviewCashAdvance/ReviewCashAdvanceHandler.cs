using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class ReviewCashAdvanceHandler(ITenantUnitOfWork tenantUow, ICurrentUserService currentUser)
    : IRequestHandler<ReviewCashAdvanceCommand, Result>
{
    public async Task<Result> Handle(ReviewCashAdvanceCommand req, CancellationToken ct)
    {
        var advance = await tenantUow.Repository<CashAdvance>().GetByIdAsync(req.Id, ct);
        if (advance is null)
        {
            return Result.Fail($"Could not find a cash advance with ID '{req.Id}'");
        }

        if (advance.Status != CashAdvanceStatus.Requested)
        {
            return Result.Fail("Only a requested advance can be approved or rejected.");
        }

        var reviewerId = currentUser.GetUserId() ?? Guid.Empty;
        if (req.Approve)
        {
            advance.Approve(reviewerId);
        }
        else
        {
            advance.Reject(reviewerId);
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
