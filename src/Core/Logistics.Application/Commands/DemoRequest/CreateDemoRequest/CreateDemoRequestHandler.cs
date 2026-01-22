using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateDemoRequestHandler(
    IMasterUnitOfWork masterUow,
    ILogger<CreateDemoRequestHandler> logger) : IAppRequestHandler<CreateDemoRequestCommand, Result>
{
    public async Task<Result> Handle(CreateDemoRequestCommand req, CancellationToken ct)
    {
        var demoRequest = new DemoRequest
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = req.Email,
            Company = req.Company,
            Phone = req.Phone,
            FleetSize = req.FleetSize,
            Message = req.Message,
        };

        await masterUow.Repository<DemoRequest>().AddAsync(demoRequest, ct);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Created demo request from {Email} at {Company}", req.Email, req.Company);
        return Result.Ok();
    }
}
