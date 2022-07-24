namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTruckByIdHandler : RequestHandlerBase<GetTruckByIdQuery, DataResult<TruckDto>>
{
    private readonly IMainRepository<User> _userRepository;
    private readonly ITenantRepository<Truck> _truckRepository;

    public GetTruckByIdHandler(
        IMainRepository<User> userRepository,
        ITenantRepository<Truck> truckRepository)
    {
        _userRepository = userRepository;
        _truckRepository = truckRepository;
    }

    protected override async Task<DataResult<TruckDto>> HandleValidated(GetTruckByIdQuery request, CancellationToken cancellationToken)
    {
        var truckEntity = await _truckRepository.GetAsync(request.Id!);

        var loadsIds = _truckRepository.GetQuery()
            .SelectMany(i => i.Loads)
            .Select(i => i.Id)
            .ToList();

        if (truckEntity == null)
            return DataResult<TruckDto>.CreateError("Could not find the specified truck");

        var truckDriver = await _userRepository.GetAsync(i => i.Id == truckEntity.DriverId);
        
        var cargo = new TruckDto
        {
            Id = truckEntity.Id,
            TruckNumber = truckEntity.TruckNumber,
            DriverId = truckEntity.DriverId,
            DriverName = truckDriver?.GetFullName(),
            LoadIds = loadsIds
        };

        return DataResult<TruckDto>.CreateSuccess(cargo);
    }

    protected override bool Validate(GetTruckByIdQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
