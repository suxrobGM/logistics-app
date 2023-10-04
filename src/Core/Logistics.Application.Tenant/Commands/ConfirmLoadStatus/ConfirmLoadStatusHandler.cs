namespace Logistics.Application.Tenant.Commands;

internal sealed class ConfirmLoadStatusHandler : RequestHandler<ConfirmLoadStatusCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public ConfirmLoadStatusHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        ConfirmLoadStatusCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantRepository.GetAsync<Load>(req.LoadId);
        
        if (load is null)
            return ResponseResult.CreateError($"Could not find load with ID '{req.LoadId}'");

        load.SetStatus(req.LoadStatus!.Value);
        _tenantRepository.Update(load);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
