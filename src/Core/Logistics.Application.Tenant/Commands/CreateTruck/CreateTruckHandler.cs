namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateTruckHandler : RequestHandlerBase<CreateTruckCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public CreateTruckHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        CreateTruckCommand request, CancellationToken cancellationToken)
    {
        var driver = await _tenantRepository.GetAsync<Employee>(request.DriverId);

        if (driver == null)
            return ResponseResult.CreateError("Could not find the specified driver");

        var truckWithThisDriver = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == request.DriverId);
        var truckWithThisNumber = await _tenantRepository.GetAsync<Truck>(i => i.TruckNumber == request.TruckNumber);

        if (truckWithThisDriver != null)
            return ResponseResult.CreateError("Already exists truck with this driver");

        if (truckWithThisNumber != null)
            return ResponseResult.CreateError("Already exists truck with this number");

        var truckEntity = new Truck()
        {
            TruckNumber = request.TruckNumber ?? 100,
            Driver = driver
        };
        
        await _tenantRepository.AddAsync(truckEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }

    protected override bool Validate(CreateTruckCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (request.TruckNumber is null)
        {
            errorDescription = "Truck number is not specified";
        }
        else if (request.TruckNumber < 0)
        {
            errorDescription = "Truck number should be non-negative";
        }
        else if (string.IsNullOrEmpty(request.DriverId))
        {
            errorDescription = "DriverId is not specified";
        }
        
        return string.IsNullOrEmpty(errorDescription);
    }
}
