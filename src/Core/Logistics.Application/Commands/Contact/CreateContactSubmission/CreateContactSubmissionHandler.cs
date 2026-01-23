using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateContactSubmissionHandler(
    IMasterUnitOfWork masterUow,
    ILogger<CreateContactSubmissionHandler> logger) : IAppRequestHandler<CreateContactSubmissionCommand, Result>
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

        return Result.Ok();
    }
}
