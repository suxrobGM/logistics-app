namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateTruckHandler : RequestHandler<CreateTruckCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public CreateTruckHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        CreateTruckCommand req, CancellationToken cancellationToken)
    {
        // var driver = await _tenantRepository.GetAsync<Employee>(request.DriverId);
        //
        // if (driver == null)
        //     return ResponseResult.CreateError("Could not find the specified driver");

        
        var truckWithThisNumber = await _tenantRepository.GetAsync<Truck>(i => i.TruckNumber == req.TruckNumber);

        if (truckWithThisNumber != null)
            return ResponseResult.CreateError("Already exists truck with this number");
        
        var drivers = _tenantRepository.ApplySpecification(new GetEmployeesById(req.DriversIds)).ToList();
        
        if (!drivers.Any())
            return ResponseResult.CreateError("Could not find any drivers with specified IDs");

        var hasAlreadyAssociatedDriver = drivers.Any(i => i.Truck != null && i.Truck.TruckNumber == req.TruckNumber);

        var truckEntity = new Truck()
        {
            TruckNumber = req.TruckNumber,
            Drivers = drivers
        };
        
        await _tenantRepository.AddAsync(truckEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
