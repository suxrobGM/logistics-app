using Logistics.Application.Abstractions;
using Logistics.Application.Extensions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteLoadHandler : RequestHandler<DeleteLoadCommand, Result>
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ITenantUnitOfWork _tenantUow;

    public DeleteLoadHandler(
        ITenantUnitOfWork tenantUow,
        IPushNotificationService pushNotificationService)
    {
        _tenantUow = tenantUow;
        _pushNotificationService = pushNotificationService;
    }

    public override async Task<Result> Handle(
        DeleteLoadCommand req, CancellationToken ct)
    {
        var load = await _tenantUow.Repository<Load>().GetByIdAsync(req.Id);

        _tenantUow.Repository<Load>().Delete(load);
        var changes = await _tenantUow.SaveChangesAsync();

        if (load is not null && changes > 0)
        {
            await _pushNotificationService.SendRemovedLoadNotificationAsync(load);
        }

        return Result.Succeed();
    }
}
