using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class DeleteContactSubmissionHandler(
    IMasterUnitOfWork masterUow,
    ILogger<DeleteContactSubmissionHandler> logger) : IAppRequestHandler<DeleteContactSubmissionCommand, Result>
{
    public async Task<Result> Handle(DeleteContactSubmissionCommand req, CancellationToken ct)
    {
        var submission = await masterUow.Repository<ContactSubmission>().GetByIdAsync(req.Id, ct);

        if (submission is null)
        {
            return Result.Fail($"Contact submission with ID '{req.Id}' not found");
        }

        masterUow.Repository<ContactSubmission>().Delete(submission);
        await masterUow.SaveChangesAsync(ct);

        logger.LogInformation("Deleted contact submission {Id}", req.Id);
        return Result.Ok();
    }
}
