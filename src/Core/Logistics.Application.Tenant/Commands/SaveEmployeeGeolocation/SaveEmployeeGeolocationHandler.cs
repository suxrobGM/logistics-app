namespace Logistics.Application.Tenant.Commands;

internal sealed class SaveEmployeeGeolocationHandler : RequestHandler<SaveEmployeeGeolocationCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public SaveEmployeeGeolocationHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        SaveEmployeeGeolocationCommand req, CancellationToken cancellationToken)
    {
        if (req.GeolocationData is null)
            return ResponseResult.CreateSuccess();
        
        _tenantRepository.SetTenantId(req.GeolocationData.TenantId);
        var employee = await _tenantRepository.GetAsync<Employee>(req.GeolocationData.UserId);

        if (employee is null)
            return ResponseResult.CreateSuccess();
        
        employee.LastKnownCoordinates = $"{req.GeolocationData.Latitude},{req.GeolocationData.Longitude}";
        _tenantRepository.Update(employee);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
