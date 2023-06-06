namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateTruckHandler : RequestHandlerBase<UpdateTruckCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateTruckHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateTruckCommand req, CancellationToken cancellationToken)
    {
        var driver = await _tenantRepository.GetAsync<Employee>(req.DriverId!);

        if (req.DriverId != null && driver == null)
            return ResponseResult.CreateError("Could not find the specified driver");
        
        var truckEntity = await _tenantRepository.GetAsync<Truck>(req.Id!);

        if (truckEntity == null)
            return ResponseResult.CreateError("Could not find the specified truck");
        
        var truckWithThisDriver = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == req.DriverId);
        var truckWithThisNumber = await _tenantRepository.GetAsync<Truck>(i => i.TruckNumber == req.TruckNumber && 
                                                                               i.Id != truckEntity.Id);

        if (truckWithThisDriver != null)
            return ResponseResult.CreateError("Already exists truck with this driver");

        if (truckWithThisNumber != null)
            return ResponseResult.CreateError("Already exists truck with this number");

        if (req.TruckNumber.HasValue)
            truckEntity.TruckNumber = req.TruckNumber.Value;

        if (driver != null)
            truckEntity.Driver = driver;

        _tenantRepository.Update(truckEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }

    protected override bool Validate(UpdateTruckCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
