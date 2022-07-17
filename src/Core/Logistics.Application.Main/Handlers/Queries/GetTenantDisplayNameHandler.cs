namespace Logistics.Application.Handlers.Queries;

internal class GetTenantDisplayNameHandler : RequestHandlerBase<GetTenantDisplayNameQuery, DataResult<TenantDto>>
{
    private readonly IMainRepository<Tenant> _repository;

    public GetTenantDisplayNameHandler(IMainRepository<Tenant> repository)
    {
        _repository = repository;
    }

    protected override async Task<DataResult<TenantDto>> HandleValidated(GetTenantDisplayNameQuery request, CancellationToken cancellationToken)
    {
        var tenantEntity = await _repository.GetAsync(i => i.Id == request.Id || i.Name == request.Name);

        if (tenantEntity == null)
        {
            return DataResult<TenantDto>.CreateError("Could not find the specified tenant");
        }

        var tenant = new TenantDto
        {
            Id = tenantEntity.Id,
            Name = tenantEntity.Name,
            DisplayName = tenantEntity.DisplayName
        };
        return DataResult<TenantDto>.CreateSuccess(tenant);
    }

    protected override bool Validate(GetTenantDisplayNameQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id) && string.IsNullOrEmpty(request.Name))
        {
            errorDescription = "Both tenant's ID and tenant's name are an empty string, specify at least either ID or tenant's name";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
