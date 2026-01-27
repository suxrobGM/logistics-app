using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class ReportLoadExceptionHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService) : IAppRequestHandler<ReportLoadExceptionCommand, Result>
{
    public async Task<Result> Handle(ReportLoadExceptionCommand req, CancellationToken ct)
    {
        var userId = currentUserService.GetUserId();
        if (!userId.HasValue)
        {
            return Result.Fail("User not authenticated");
        }

        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result.Fail($"Load not found with ID '{req.LoadId}'");
        }

        var exception = new LoadException
        {
            LoadId = req.LoadId,
            Type = req.Type,
            Reason = req.Reason,
            OccurredAt = DateTime.UtcNow,
            ReportedById = userId.Value
        };

        await tenantUow.Repository<LoadException>().AddAsync(exception, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
