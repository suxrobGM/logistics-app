using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.Contacts.Queries;

internal sealed class GetContactSubmissionsHandler(
    IMasterUnitOfWork masterUow) : IRequestHandler<GetContactSubmissionsQuery, PagedResult<ContactSubmissionDto>>
{
    private static readonly SortFields<ContactSubmission> Sorts = new SortFields<ContactSubmission>("-createdAt")
        .Add("firstName", x => x.FirstName)
        .Add("email", x => x.Email)
        .Add("subject", x => x.Subject)
        .Add("status", x => x.Status)
        .Add("createdAt", x => x.CreatedAt);

    public async Task<PagedResult<ContactSubmissionDto>> Handle(GetContactSubmissionsQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<ContactSubmission>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(x =>
                x.Email.ToLower().Contains(search) ||
                x.FirstName.ToLower().Contains(search) ||
                x.LastName.ToLower().Contains(search) ||
                x.Message.ToLower().Contains(search));
        }

        return await query.ToPagedResultAsync(req, Sorts, i => i.ToDto(), ct);
    }
}
