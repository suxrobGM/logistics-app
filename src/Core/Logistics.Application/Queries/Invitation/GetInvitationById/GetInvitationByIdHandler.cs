using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetInvitationByIdHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetInvitationByIdQuery, Result<InvitationDto>>
{
    public async Task<Result<InvitationDto>> Handle(GetInvitationByIdQuery req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        var invitation = await masterUow.Repository<Invitation>()
            .Query()
            .FirstOrDefaultAsync(i => i.Id == req.Id && i.TenantId == tenant.Id, ct);

        if (invitation is null)
        {
            return Result<InvitationDto>.Fail("Invitation not found.");
        }

        string? customerName = null;
        if (invitation.CustomerId.HasValue)
        {
            var customer = await tenantUow.Repository<Customer>().GetByIdAsync(invitation.CustomerId.Value, ct);
            customerName = customer?.Name;
        }

        var dto = invitation.ToDto(
            tenant.CompanyName ?? tenant.Name,
            invitation.InvitedByUser?.GetFullName() ?? string.Empty,
            customerName);

        return Result<InvitationDto>.Ok(dto);
    }
}
