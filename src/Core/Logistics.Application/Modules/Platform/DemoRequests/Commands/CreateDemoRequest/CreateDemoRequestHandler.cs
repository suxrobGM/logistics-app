using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Modules.Platform.Contacts.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Platform.DemoRequests.Commands;

internal sealed class CreateDemoRequestHandler(
    IMasterUnitOfWork masterUow,
    ICommandEnqueuer commandEnqueuer,
    ILogger<CreateDemoRequestHandler> logger) : IRequestHandler<CreateDemoRequestCommand, Result>
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

        var name = $"{req.FirstName} {req.LastName}";
        commandEnqueuer.Enqueue(new SendWebsiteLeadEmailCommand
        {
            Subject = $"Demo request: {name} ({req.Company})",
            ReplyTo = req.Email,
            Email = new WebsiteLeadEmailModel
            {
                Heading = "New demo request",
                Fields =
                [
                    new("Name", name),
                    new("Email", req.Email),
                    new("Company", req.Company),
                    new("Phone", req.Phone ?? "-"),
                    new("Fleet size", req.FleetSize ?? "-"),
                ],
                Message = req.Message,
            },
        });

        return Result.Ok();
    }
}
