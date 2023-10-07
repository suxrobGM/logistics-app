using Logistics.Application.Common;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Models;
using Logistics.Shared;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetTenantHandler : RequestHandler<GetTenantQuery, ResponseResult<TenantDto>>
{
    private readonly IMainRepository _repository;

    public GetTenantHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override async Task<ResponseResult<TenantDto>> HandleValidated(
        GetTenantQuery req, CancellationToken cancellationToken)
    {
        var tenantEntity = await _repository.GetAsync<Tenant>(i => i.Id == req.Id || i.Name == req.Name);

        if (tenantEntity == null)
            return ResponseResult<TenantDto>.CreateError("Could not find the specified tenant");
        
        var tenant = new TenantDto
        {
            Id = tenantEntity.Id,
            Name = tenantEntity.Name,
            DisplayName = tenantEntity.DisplayName,
            ConnectionString = req.IncludeConnectionString ? tenantEntity.ConnectionString : default
        };

        return ResponseResult<TenantDto>.CreateSuccess(tenant);
    }
}
