using Logistics.Application.Tenant.Extensions;
using Logistics.Application.Tenant.Services;

namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteLoadHandler : RequestHandler<DeleteLoadCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPushNotificationService _pushNotificationService;

    public DeleteLoadHandler(
        ITenantRepository tenantRepository,
        IPushNotificationService pushNotificationService)
    {
        _tenantRepository = tenantRepository;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeleteLoadCommand req, CancellationToken cancellationToken)
    {
        var load = await _tenantRepository.GetAsync<Load>(req.Id);
        _tenantRepository.Delete(load);
        var changes = await _tenantRepository.UnitOfWork.CommitAsync();
        
        if (load != null && changes > 0)
        {
            await _pushNotificationService.SendRemovedLoadNotificationAsync(load, load.AssignedTruck!);
        }
        
        return ResponseResult.CreateSuccess();
    }
}
