using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

internal sealed class SetDriverDeviceTokenHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<SetDriverDeviceTokenCommand, Result>
{
    public async Task<Result> Handle(
        SetDriverDeviceTokenCommand req, CancellationToken ct)
    {
        var driver = await tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

        if (driver is null)
        {
            return Result.Fail("Could not find the specified driver");
        }

        if (!string.IsNullOrEmpty(driver.DeviceToken) && driver.DeviceToken == req.DeviceToken)
        {
            return Result.Ok();
        }

        driver.DeviceToken = req.DeviceToken;
        tenantUow.Repository<Employee>().Update(driver);
        await tenantUow.SaveChangesAsync();
        return Result.Ok();
    }
}
