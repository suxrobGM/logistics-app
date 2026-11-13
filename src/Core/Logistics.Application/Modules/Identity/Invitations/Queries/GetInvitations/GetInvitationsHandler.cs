using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Identity.Invitations.Queries;

internal sealed class GetInvitationsHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetInvitationsQuery, PagedResult<InvitationDto>>
{
    public async Task<PagedResult<InvitationDto>> Handle(GetInvitationsQuery req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        var query = masterUow.Repository<Invitation>()
            .Query()
            .Where(i => i.TenantId == tenant.Id);

        if (req.Status.HasValue)
        {
            query = query.Where(i => i.Status == req.Status.Value);
        }

        if (req.Type.HasValue)
        {
            query = query.Where(i => i.Type == req.Type.Value);
        }

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(i => i.Email.ToLower().Contains(search));
        }

        return await query.ToPagedResultAsync(
            req, (page, token) => MapPageAsync(page, tenant, token), ct, defaultOrderBy: "-CreatedAt");
    }

    private async Task<IEnumerable<InvitationDto>> MapPageAsync(
        IReadOnlyList<Invitation> invitations,
        Tenant tenant,
        CancellationToken ct)
    {
        var customerIds = invitations
            .Where(i => i.CustomerId.HasValue)
            .Select(i => i.CustomerId!.Value)
            .Distinct()
            .ToList();

        var customerNames = new Dictionary<Guid, string>();
        if (customerIds.Any())
        {
            customerNames = await tenantUow.Repository<Customer>()
                .Query()
                .Where(c => customerIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Name })
                .ToDictionaryAsync(c => c.Id, c => c.Name, ct);
        }

        // Same format as User.GetFullName, which can't run inside the query.
        var inviterIds = invitations.Select(i => i.InvitedByUserId).Distinct().ToArray();
        var inviterNames = (await masterUow.Repository<User>()
                .Query()
                .Where(u => inviterIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToListAsync(ct))
            .ToDictionary(u => u.Id, u => string.Join(" ", u.FirstName, u.LastName));

        return invitations.Select(i =>
        {
            var customerName = i.CustomerId.HasValue && customerNames.TryGetValue(i.CustomerId.Value, out var name)
                ? name
                : null;

            return i.ToDto(
                tenant.DisplayName,
                inviterNames.GetValueOrDefault(i.InvitedByUserId, string.Empty),
                customerName);
        });
    }
}
