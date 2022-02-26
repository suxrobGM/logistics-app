namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTrucksQueryHandler : GetPagedQueryHandlerBase<GetTrucksQuery, TruckDto>
{
    private readonly IRepository<Cargo> cargoRepository;
    private readonly IRepository<Truck> truckRepository;

    public GetTrucksQueryHandler(
        IRepository<Cargo> cargoRepository,
        IRepository<Truck> truckRepository)
    {
        this.cargoRepository = cargoRepository;
        this.truckRepository = truckRepository;
    }

    protected override Task<PagedDataResult<TruckDto>> HandleValidated(
        GetPagedQueryBase<TruckDto> request, 
        CancellationToken cancellationToken)
    {
        var cargoesIdsList = cargoRepository.GetQuery().Select(i => i.Id).ToList();
        var totalItems = truckRepository.GetQuery().Count();

        var items = truckRepository.GetQuery()
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new TruckDto
            {
                TruckNumber = i.TruckNumber,
                DriverId = i.DriverId,
                CargoesIds = cargoesIdsList
            })
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<TruckDto>(items, totalItems, totalPages));
    }
}
