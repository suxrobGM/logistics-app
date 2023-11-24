namespace Logistics.Application.Tenant.Commands;

internal sealed class SetDriverDeviceTokenHandler : RequestHandler<SetDriverDeviceTokenCommand, ResponseResult>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public SetDriverDeviceTokenHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult> HandleValidated(
        SetDriverDeviceTokenCommand req, CancellationToken cancellationToken)
    {
        var driver = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

        if (driver is null)
        {
            return ResponseResult.CreateError("Could not find the specified driver");
        }

        if (!string.IsNullOrEmpty(driver.DeviceToken) && driver.DeviceToken == req.DeviceToken)
        {
            return ResponseResult.CreateSuccess();
        }

        driver.DeviceToken = req.DeviceToken;
        _tenantUow.Repository<Employee>().Update(driver);
        await _tenantUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
