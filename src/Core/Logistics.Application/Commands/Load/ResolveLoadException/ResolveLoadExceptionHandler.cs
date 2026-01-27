using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class ResolveLoadExceptionHandler(ITenantUnitOfWork tenantUow) : IAppRequestHandler<ResolveLoadExceptionCommand, Result>
{
    public async Task<Result> Handle(ResolveLoadExceptionCommand req, CancellationToken ct)
    {
        var exception = await tenantUow.Repository<LoadException>()
            .GetAsync(e => e.Id == req.ExceptionId && e.LoadId == req.LoadId, ct);

        if (exception is null)
        {
            return Result.Fail($"Exception not found with ID '{req.ExceptionId}'");
        }

        if (exception.IsResolved)
        {
            return Result.Fail("This exception has already been resolved");
        }

        exception.ResolvedAt = DateTime.UtcNow;
        exception.Resolution = req.Resolution;

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
