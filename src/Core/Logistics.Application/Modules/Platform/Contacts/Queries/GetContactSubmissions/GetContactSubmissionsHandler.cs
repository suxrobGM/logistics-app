using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Contacts.Queries;

internal sealed class GetContactSubmissionsHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetContactSubmissionsQuery, PagedResult<ContactSubmissionDto>>
{
    public Task<PagedResult<ContactSubmissionDto>> Handle(GetContactSubmissionsQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<ContactSubmission>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(x =>
                x.Email.Contains(req.Search) ||
                x.FirstName.Contains(req.Search) ||
                x.LastName.Contains(req.Search) ||
                x.Message.Contains(req.Search));
        }

        var totalItems = query.Count();

        var items = query
            .OrderBy(req.OrderBy)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<ContactSubmissionDto>.Ok(items, totalItems, req.PageSize));
    }
}
