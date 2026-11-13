using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Modules.Identity.Invitations;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Identity.Admins.Queries;

internal sealed class GetAdminInvitationsHandler(IMasterUnitOfWork masterUow)
    : IRequestHandler<GetAdminInvitationsQuery, PagedResult<InvitationDto>>
{
    public async Task<PagedResult<InvitationDto>> Handle(GetAdminInvitationsQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<Invitation>()
            .Query()
            .Where(i => i.Type == InvitationType.AppUser && i.Status == InvitationStatus.Pending);

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(i => i.Email.ToLower().Contains(search));
        }

        return await query.ToPagedResultAsync(req, MapPageAsync, ct, defaultOrderBy: "-CreatedAt");
    }

    private async Task<IEnumerable<InvitationDto>> MapPageAsync(IReadOnlyList<Invitation> invitations, CancellationToken ct)
    {
        // Same format as User.GetFullName, which can't run inside the query.
        var inviterIds = invitations.Select(i => i.InvitedByUserId).Distinct().ToArray();
        var inviterNames = (await masterUow.Repository<User>()
                .Query()
                .Where(u => inviterIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToListAsync(ct))
            .ToDictionary(u => u.Id, u => string.Join(" ", u.FirstName, u.LastName));

        // App-user invitations have no tenant.
        return invitations.Select(i => i.ToDto(string.Empty, inviterNames.GetValueOrDefault(i.InvitedByUserId, string.Empty)));
    }
}
