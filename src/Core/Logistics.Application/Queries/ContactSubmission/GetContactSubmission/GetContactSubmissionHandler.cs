using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetContactSubmissionHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetContactSubmissionQuery, Result<ContactSubmissionDto>>
{
    public async Task<Result<ContactSubmissionDto>> Handle(GetContactSubmissionQuery req, CancellationToken ct)
    {
        var submission = await masterUow.Repository<ContactSubmission>().GetByIdAsync(req.Id, ct);

        if (submission is null)
        {
            return Result<ContactSubmissionDto>.Fail($"Contact submission with ID '{req.Id}' not found");
        }

        return Result<ContactSubmissionDto>.Ok(submission.ToDto());
    }
}
