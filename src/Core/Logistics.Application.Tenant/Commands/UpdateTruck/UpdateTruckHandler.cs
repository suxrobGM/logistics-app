namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateTruckHandler : RequestHandler<UpdateTruckCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateTruckHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateTruckCommand req, CancellationToken cancellationToken)
    {
        var truckEntity = await _tenantRepository.GetAsync<Truck>(req.Id!);

        if (truckEntity == null)
            return ResponseResult.CreateError("Could not find the specified truck");
        
        var truckWithThisNumber = await _tenantRepository.GetAsync<Truck>(i => i.TruckNumber == req.TruckNumber && 
                                                                               i.Id != truckEntity.Id);
        if (truckWithThisNumber != null)
            return ResponseResult.CreateError("Already exists truck with this number");
        
        if (req.DriverIds != null)
        {
            var drivers = _tenantRepository.ApplySpecification(new GetEmployeesById(req.DriverIds)).ToList();
            
            if (drivers.Any())
                truckEntity.Drivers = drivers;
        }
        
        if (!string.IsNullOrEmpty(req.TruckNumber))
            truckEntity.TruckNumber = req.TruckNumber;

        _tenantRepository.Update(truckEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
