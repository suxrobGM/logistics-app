using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DeleteDemoRequestHandler(
    IMasterUnitOfWork masterUow,
    ILogger<DeleteDemoRequestHandler> logger) : IAppRequestHandler<DeleteDemoRequestCommand, Result>
{
    public async Task<Result> Handle(DeleteDemoRequestCommand req, CancellationToken ct)
    {
        var demoRequest = await masterUow.Repository<DemoRequest>().GetByIdAsync(req.Id, ct);

        if (demoRequest is null)
        {
            return Result.Fail($"Demo request with ID '{req.Id}' not found");
        }

        masterUow.Repository<DemoRequest>().Delete(demoRequest);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Deleted demo request {Id}", req.Id);
        return Result.Ok();
    }
}
