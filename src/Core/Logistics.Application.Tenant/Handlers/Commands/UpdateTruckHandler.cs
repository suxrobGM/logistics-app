namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateTruckHandler : RequestHandlerBase<UpdateTruckCommand, DataResult>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateTruckHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        UpdateTruckCommand req, CancellationToken cancellationToken)
    {
        var driver = await _tenantRepository.GetAsync<Employee>(req.DriverId!);

        if (driver == null)
            return DataResult.CreateError("Could not find the specified driver");
        
        var truckEntity = await _tenantRepository.GetAsync<Truck>(req.Id!);

        if (truckEntity == null)
            return DataResult.CreateError("Could not find the specified truck");
        
        var truckWithThisDriver = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == req.DriverId);
        var truckWithThisNumber = await _tenantRepository.GetAsync<Truck>(i => i.TruckNumber == req.TruckNumber && 
                                                                               i.TruckNumber != truckEntity.TruckNumber);

        if (truckWithThisDriver != null)
            return DataResult.CreateError("Already exists truck with this driver");

        if (truckWithThisNumber != null)
            return DataResult.CreateError("Already exists truck with this number");

        if (req.TruckNumber.HasValue)
        {
            truckEntity.TruckNumber = req.TruckNumber.Value;
        }
        
        truckEntity.Driver = driver;
        _tenantRepository.Update(truckEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(UpdateTruckCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }
        else if (string.IsNullOrEmpty(request.DriverId))
        {
            errorDescription = "Truck driver id is an empty string";
        }
        
        return string.IsNullOrEmpty(errorDescription);
    }
}
