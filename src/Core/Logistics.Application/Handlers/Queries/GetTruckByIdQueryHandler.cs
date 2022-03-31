namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTruckByIdQueryHandler : RequestHandlerBase<GetTruckByIdQuery, DataResult<TruckDto>>
{
    private readonly IRepository<Truck> truckRepository;

    public GetTruckByIdQueryHandler(IRepository<Truck> truckRepository)
    {
        this.truckRepository = truckRepository;
    }

    protected override async Task<DataResult<TruckDto>> HandleValidated(GetTruckByIdQuery request, CancellationToken cancellationToken)
    {
        var truckEntity = await truckRepository.GetAsync(request.Id!);

        var cargoesIdsList = truckRepository.GetQuery()
            .SelectMany(i => i.Cargoes)
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
            CargoesIds = cargoesIdsList
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
