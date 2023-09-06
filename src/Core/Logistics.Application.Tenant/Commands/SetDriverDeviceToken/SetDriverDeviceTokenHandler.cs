namespace Logistics.Application.Tenant.Commands;

internal sealed class SetDriverDeviceTokenHandler : RequestHandler<SetDriverDeviceTokenCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public SetDriverDeviceTokenHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        SetDriverDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var driver = await _tenantRepository.GetAsync<Employee>(request.UserId);

        if (driver == null)
            return ResponseResult.CreateError("Could not find the specified driver");

        if (!string.IsNullOrEmpty(driver.DeviceToken) && driver.DeviceToken == request.DeviceToken)
        {
            return ResponseResult.CreateSuccess();
        }

        driver.DeviceToken = request.DeviceToken;
        _tenantRepository.Update(driver);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
