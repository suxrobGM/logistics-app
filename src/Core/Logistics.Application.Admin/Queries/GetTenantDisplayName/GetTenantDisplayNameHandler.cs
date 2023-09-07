using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

internal sealed class GetTenantDisplayNameHandler : RequestHandler<GetTenantDisplayNameQuery, ResponseResult<TenantDto>>
{
    private readonly IMainRepository _repository;

    public GetTenantDisplayNameHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override async Task<ResponseResult<TenantDto>> HandleValidated(GetTenantDisplayNameQuery req, CancellationToken cancellationToken)
    {
        var tenantEntity = await _repository.GetAsync<Domain.Entities.Tenant>(i => i.Id == req.Id || i.Name == req.Name);

        if (tenantEntity == null)
            return ResponseResult<TenantDto>.CreateError("Could not find the specified tenant");

        var tenant = new TenantDto
        {
            Id = tenantEntity.Id,
            Name = tenantEntity.Name,
            DisplayName = tenantEntity.DisplayName
        };
        return ResponseResult<TenantDto>.CreateSuccess(tenant);
    }
}
