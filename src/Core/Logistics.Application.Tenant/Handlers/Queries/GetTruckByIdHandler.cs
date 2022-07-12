namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTruckByIdHandler : RequestHandlerBase<GetTruckByIdQuery, DataResult<TruckDto>>
{
    private readonly ITenantRepository<Truck> _truckRepository;

    public GetTruckByIdHandler(ITenantRepository<Truck> truckRepository)
    {
        _truckRepository = truckRepository;
    }

    protected override async Task<DataResult<TruckDto>> HandleValidated(GetTruckByIdQuery request, CancellationToken cancellationToken)
    {
        var truckEntity = await _truckRepository.GetAsync(request.Id!);

        var cargoesIdsList = _truckRepository.GetQuery()
            .SelectMany(i => i.Loads)
            .Select(i => i.Id)
            .ToList();

        if (truckEntity == null)
        {
            return DataResult<TruckDto>.CreateError("Could not find the specified truck");
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var cargo = new TruckDto
        {
            Id = truckEntity.Id,
            TruckNumber = truckEntity.TruckNumber,
            DriverId = truckEntity.DriverId,
            DriverName = truckEntity.Driver.GetFullName(),
            LoadIds = cargoesIdsList
        };
#pragma warning restore CS8602 // Dereference of a possibly null reference.

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
