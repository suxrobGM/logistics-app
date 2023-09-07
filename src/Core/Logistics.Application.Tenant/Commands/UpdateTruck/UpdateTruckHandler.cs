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
        
        var drivers = _tenantRepository.ApplySpecification(new GetEmployeesById(req.DriversIds)).ToList();
        
        if (!string.IsNullOrEmpty(req.TruckNumber))
            truckEntity.TruckNumber = req.TruckNumber;

        if (drivers.Any())
            truckEntity.Drivers = drivers;

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
