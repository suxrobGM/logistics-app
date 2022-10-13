namespace Logistics.Application.Main.Handlers.Commands;

internal sealed class DeleteTenantHandler : RequestHandlerBase<DeleteTenantCommand, ResponseResult>
{
    private readonly IDatabaseProviderService _databaseProvider;
    private readonly IMainRepository _repository;

    public DeleteTenantHandler(
        IDatabaseProviderService databaseProvider,
        IMainRepository repository)
    {
        _databaseProvider = databaseProvider;
        _repository = repository;
    }

    protected override async Task<ResponseResult> HandleValidated(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetAsync<Tenant>(request.Id!);

        if (tenant == null)
            return ResponseResult.CreateError("Could not find the tenant");

        var deleted = await _databaseProvider.DeleteDatabaseAsync(tenant.ConnectionString!);

        if (!deleted)
            return ResponseResult.CreateError("Could not delete the tenant's database");

        _repository.Delete(tenant);
        await _repository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
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
