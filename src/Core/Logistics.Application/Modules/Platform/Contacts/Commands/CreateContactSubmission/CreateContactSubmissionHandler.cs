using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Modules.Platform.Contacts.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Platform.Contacts.Commands;

internal sealed class CreateContactSubmissionHandler(
    IMasterUnitOfWork masterUow,
    IWebsiteLeadNotifier leadNotifier,
    ILogger<CreateContactSubmissionHandler> logger) : IRequestHandler<CreateContactSubmissionCommand, Result>
{
    public async Task<Result> Handle(CreateContactSubmissionCommand req, CancellationToken ct)
    {
        var submission = new ContactSubmission
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = req.Email,
            Phone = req.Phone,
            Subject = req.Subject,
            Message = req.Message,
        };

        await masterUow.Repository<ContactSubmission>().AddAsync(submission, ct);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created contact submission from {Email} with subject {Subject}",
            req.Email,
            req.Subject);

        var name = $"{req.FirstName} {req.LastName}";
        var subject = req.Subject.GetDescription();
        await leadNotifier.NotifyAsync(
            $"Contact form ({subject}): {name}",
            req.Email,
            new WebsiteLeadEmailModel
            {
                Heading = "New contact message",
                Fields =
                [
                    new("Name", name),
                    new("Email", req.Email),
                    new("Phone", req.Phone ?? "-"),
                    new("Subject", subject),
                ],
                Message = req.Message,
            },
            ct);

        return Result.Ok();
    }
}
