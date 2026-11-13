using Logistics.Application.Abstractions;
using Logistics.Application.Modules.IdentityAccess.Tenants;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Users.Queries;

internal sealed class GetUserCurrentTenantHandler(IMasterUnitOfWork masterUow) :
    IRequestHandler<GetUserCurrentTenantQuery, Result<TenantDto>>
{
    public async Task<Result<TenantDto>> Handle(
        GetUserCurrentTenantQuery req,
        CancellationToken ct)
    {
        var user = await masterUow.Repository<User>().GetByIdAsync(req.UserId, ct);

        if (user is null)
        {
            return Result<TenantDto>.Fail($"Could not find an user with ID '{req.UserId}'", ErrorCodes.NotFound);
        }

        if (user.Tenant is null)
        {
            return Result<TenantDto>.Fail($"User with ID '{req.UserId}' does not have a tenant", ErrorCodes.NotFound);
        }

        var tenantDto = user.Tenant.ToDto();
        return Result<TenantDto>.Ok(tenantDto);
    }
}
