namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateTenantHandler : RequestHandler<UpdateTenantCommand, ResponseResult>
{
    private readonly IMainRepository _repository;

    public UpdateTenantHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override async Task<ResponseResult> HandleValidated(UpdateTenantCommand req, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetAsync<Domain.Entities.Tenant>(req.Id!);

        if (tenant == null)
            return ResponseResult.CreateError("Could not find the tenant");

        tenant.Name = req.Name?.Trim().ToLower();
        tenant.DisplayName = string.IsNullOrEmpty(req.DisplayName) ? tenant.Name : req.DisplayName?.Trim();
        
        _repository.Update(tenant);
        await _repository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
