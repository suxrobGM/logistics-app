namespace Logistics.Application.Handlers.Commands;

internal sealed class DeleteTenantHandler : RequestHandlerBase<DeleteTenantCommand, DataResult>
{
    private readonly IDatabaseProviderService _databaseProvider;
    private readonly IMainRepository<Tenant> _repository;

    public DeleteTenantHandler(
        IDatabaseProviderService databaseProvider,
        IMainRepository<Tenant> repository)
    {
        _databaseProvider = databaseProvider;
        _repository = repository;
    }

    protected override async Task<DataResult> HandleValidated(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetAsync(request.Id!);

        if (tenant == null)
        {
            return DataResult.CreateError("Could not find the tenant");
        }

        var deleted = await _databaseProvider.DeleteDatabaseAsync(tenant.ConnectionString!);

        if (!deleted)
        {
            return DataResult.CreateError("Could not delete the tenant's database");
        }

        _repository.Delete(tenant.Id);
        await _repository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(DeleteTenantCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
