using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetUserCurrentTenantHandler :
    RequestHandler<GetUserCurrentTenantQuery, Result<TenantDto>>
{
    private readonly IMasterUnityOfWork _masterUow;

    public GetUserCurrentTenantHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result<TenantDto>> HandleValidated(
        GetUserCurrentTenantQuery req,
        CancellationToken ct)
    {
        var user = await _masterUow.Repository<User>().GetByIdAsync(req.UserId);

        if (user is null) return Result<TenantDto>.Fail($"Could not find an user with ID '{req.UserId}'");

        if (user.Tenant is null) return Result<TenantDto>.Fail($"User with ID '{req.UserId}' does not have a tenant");

        var tenantDto = user.Tenant.ToDto();
        return Result<TenantDto>.Succeed(tenantDto);
    }
}
