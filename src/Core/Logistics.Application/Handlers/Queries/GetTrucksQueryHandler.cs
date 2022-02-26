namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTrucksQueryHandler : RequestHandlerBase<GetTrucksQuery, PagedDataResult<TruckDto>>
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
        GetTrucksQuery request, 
        CancellationToken cancellationToken)
    {
        var cargoesIdsList = cargoRepository.GetQuery().Select(i => i.Id).ToList();
        var totalItems = truckRepository.GetQuery().Count();

        var items = truckRepository.GetQuery()
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new TruckDto
            {
                Id = i.Id,
                TruckNumber = i.TruckNumber,
                DriverId = i.DriverId,
                CargoesIds = cargoesIdsList
            })
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<TruckDto>(items, totalItems, totalPages));
    }

    protected override bool Validate(GetTrucksQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (request.Page <= 0)
        {
            errorDescription = "Page number should be non-negative";
        }
        else if (request.PageSize <= 1)
        {
            errorDescription = "Page size should be greater than one";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
