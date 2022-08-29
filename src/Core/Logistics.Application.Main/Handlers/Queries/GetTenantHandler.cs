namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTenantHandler : RequestHandlerBase<GetTenantQuery, DataResult<TenantDto>>
{
    private readonly IMainRepository _repository;

    public GetTenantHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override async Task<DataResult<TenantDto>> HandleValidated(GetTenantQuery request, CancellationToken cancellationToken)
    {
        var tenantEntity = await _repository.GetAsync<Tenant>(i => i.Id == request.Id || i.Name == request.Name);

        if (tenantEntity == null)
            return DataResult<TenantDto>.CreateError("Could not find the specified tenant");
        
        var tenant = new TenantDto
        {
            Id = tenantEntity.Id,
            Name = tenantEntity.Name,
            DisplayName = tenantEntity.DisplayName,
            ConnectionString = tenantEntity.ConnectionString
        };

        return DataResult<TenantDto>.CreateSuccess(tenant);
    }

    protected override bool Validate(GetTenantQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id) && string.IsNullOrEmpty(request.Name))
        {
            errorDescription = "Both tenant's ID and tenant's name are an empty string, specify at least either ID or tenant's name";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
