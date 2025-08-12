using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class SetDriverDeviceTokenHandler : RequestHandler<SetDriverDeviceTokenCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public SetDriverDeviceTokenHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        SetDriverDeviceTokenCommand req, CancellationToken ct)
    {
        var driver = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

        if (driver is null)
        {
            return Result.Fail("Could not find the specified driver");
        }

        if (!string.IsNullOrEmpty(driver.DeviceToken) && driver.DeviceToken == req.DeviceToken)
        {
            return Result.Succeed();
        }

        driver.DeviceToken = req.DeviceToken;
        _tenantUow.Repository<Employee>().Update(driver);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
