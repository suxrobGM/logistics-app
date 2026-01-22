using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class UpdateDemoRequestHandler(
    IMasterUnitOfWork masterUow,
    ILogger<UpdateDemoRequestHandler> logger) : IAppRequestHandler<UpdateDemoRequestCommand, Result>
{
    public async Task<Result> Handle(UpdateDemoRequestCommand req, CancellationToken ct)
    {
        var demoRequest = await masterUow.Repository<DemoRequest>().GetByIdAsync(req.Id, ct);

        if (demoRequest is null)
        {
            return Result.Fail($"Demo request with ID '{req.Id}' not found");
        }

        demoRequest.Status = (DemoRequestStatus)req.Status;
        demoRequest.Notes = req.Notes;
        demoRequest.UpdatedAt = DateTime.UtcNow;

        masterUow.Repository<DemoRequest>().Update(demoRequest);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Updated demo request {Id} to status {Status}", req.Id, demoRequest.Status);
        return Result.Ok();
    }
}
