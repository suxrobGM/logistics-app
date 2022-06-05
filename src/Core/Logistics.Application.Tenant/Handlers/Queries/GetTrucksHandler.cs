namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTrucksHandler : RequestHandlerBase<GetTrucksQuery, PagedDataResult<TruckDto>>
{
    private readonly ITenantRepository<Truck> _truckRepository;

    public GetTrucksHandler(
        ITenantRepository<Truck> truckRepository)
    {
        _truckRepository = truckRepository;
    }

    protected override Task<PagedDataResult<TruckDto>> HandleValidated(
        GetTrucksQuery request, 
        CancellationToken cancellationToken)
    {
        var cargoesIdsList = new List<string>();
        if (request.IncludeCargoIds)
        {
            cargoesIdsList = _truckRepository.GetQuery()
                        .SelectMany(i => i.Cargoes)
                        .Select(i => i.Id)
                        .ToList();
        }

        var totalItems = _truckRepository.GetQuery().Count();
        var itemsQuery = _truckRepository.GetQuery();

        if (!string.IsNullOrEmpty(request.Search))
        {
            itemsQuery = _truckRepository.GetQuery(new TrucksSpecification(request.Search));
        }

        var items = itemsQuery
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .OrderBy(i => i.TruckNumber)
                .Select(i => new TruckDto
                {
                    Id = i.Id,
                    TruckNumber = i.TruckNumber,
                    DriverId = i.DriverId,
                    DriverName = i.Driver != null ? i.Driver.GetFullName() : null,
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
