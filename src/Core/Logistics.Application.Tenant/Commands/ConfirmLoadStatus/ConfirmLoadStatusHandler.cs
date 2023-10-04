using Logistics.Domain.Enums;

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

        var loadStatus = req.LoadStatus!.Value;
        load.SetStatus(loadStatus);

        switch (loadStatus)
        {
            case LoadStatus.PickedUp:
                load.CanConfirmPickUp = false;
                break;
            case LoadStatus.Delivered:
                load.CanConfirmPickUp = false;
                load.CanConfirmDelivery = false;
                break;
        }
        
        _tenantRepository.Update(load);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
