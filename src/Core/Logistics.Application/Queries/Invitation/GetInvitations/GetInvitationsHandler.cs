using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetInvitationsHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetInvitationsQuery, PagedResult<InvitationDto>>
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

        var totalItems = await query.CountAsync(ct);

        var invitations = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync(ct);

        // Get customer names for CustomerUser invitations
        var customerIds = invitations
            .Where(i => i.CustomerId.HasValue)
            .Select(i => i.CustomerId!.Value)
            .Distinct()
            .ToList();

        var customerNames = new Dictionary<Guid, string>();
        if (customerIds.Any())
        {
            var customers = await tenantUow.Repository<Customer>()
                .Query()
                .Where(c => customerIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Name })
                .ToListAsync(ct);

            customerNames = customers.ToDictionary(c => c.Id, c => c.Name);
        }

        var dtos = invitations.Select(i =>
        {
            var customerName = i.CustomerId.HasValue && customerNames.TryGetValue(i.CustomerId.Value, out var name)
                ? name
                : null;

            return i.ToDto(
                tenant.CompanyName ?? tenant.Name,
                i.InvitedByUser?.GetFullName() ?? string.Empty,
                customerName);
        }).ToList();

        return PagedResult<InvitationDto>.Succeed(dtos, totalItems, req.PageSize);
    }
}
