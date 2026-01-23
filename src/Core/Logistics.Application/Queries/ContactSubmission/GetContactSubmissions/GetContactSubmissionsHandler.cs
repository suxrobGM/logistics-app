using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetContactSubmissionsHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetContactSubmissionsQuery, PagedResult<ContactSubmissionDto>>
{
    public async Task<PagedResult<ContactSubmissionDto>> Handle(GetContactSubmissionsQuery req, CancellationToken ct)
    {
        var totalItems = await masterUow.Repository<ContactSubmission>().CountAsync(null, ct);
        var spec = new GetContactSubmissions(req.OrderBy, req.Page, req.PageSize, req.Search);

        var items = masterUow.Repository<ContactSubmission>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<ContactSubmissionDto>.Succeed(items, totalItems, req.PageSize);
    }
}
