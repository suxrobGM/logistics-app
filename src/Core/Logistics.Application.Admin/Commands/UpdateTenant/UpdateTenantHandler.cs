namespace Logistics.Application.Admin.Commands;

internal sealed class UpdateTenantHandler : RequestHandlerBase<UpdateTenantCommand, ResponseResult>
{
    private readonly IMainRepository _repository;

    public UpdateTenantHandler(IMainRepository repository)
    {
        _repository = repository;
    }

    protected override async Task<ResponseResult> HandleValidated(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetAsync<Domain.Entities.Tenant>(request.Id!);

        if (tenant == null)
            return ResponseResult.CreateError("Could not find the tenant");

        tenant.Name = request.Name?.Trim().ToLower();
        tenant.DisplayName = string.IsNullOrEmpty(request.DisplayName) ? tenant.Name : request.DisplayName?.Trim();
        
        _repository.Update(tenant);
        await _repository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
