namespace Logistics.Application.Handlers.Queries;

internal sealed class GetCargoesHandler : RequestHandlerBase<GetCargoesQuery, PagedDataResult<CargoDto>>
{
    private readonly ITenantRepository<Cargo> _cargoRepository;

    public GetCargoesHandler(ITenantRepository<Cargo> cargoRepository)
    {
        _cargoRepository = cargoRepository;
    }

    protected override Task<PagedDataResult<CargoDto>> HandleValidated(
        GetCargoesQuery request, 
        CancellationToken cancellationToken)
    {
        var totalItems = _cargoRepository.GetQuery().Count();
        var itemsQuery = _cargoRepository.GetQuery();

        if (!string.IsNullOrEmpty(request.Search))
        {
            itemsQuery = _cargoRepository.GetQuery(new CargoesSpecification(request.Search));
        }

        var items = itemsQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .OrderBy(i => i.Id)
            .Select(i => new CargoDto
            {
                Id = i.Id,
                Source = i.Source,
                Destination = i.Destination,
                PricePerMile = i.PricePerMile,
                TotalTripMiles = i.TotalTripMiles,
                PickUpDate = i.PickUpDate,
                IsCompleted = i.IsCompleted,
                AssignedDispatcherId = i.AssignedDispatcherId,
                AssignedDispatcherName = i.AssignedDispatcher != null ? i.AssignedDispatcher.GetFullName() : null,
                AssignedTruckId = i.AssignedTruck != null ? i.AssignedTruck.Id : null,
                AssignedTruckDriverName = i.AssignedTruck != null && i.AssignedTruck.Driver != null ? i.AssignedTruck.Driver.GetFullName() : null,
                Status = i.Status.ToString()
            })
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<CargoDto>(items, totalItems, totalPages));
    }

    protected override bool Validate(GetCargoesQuery request, out string errorDescription)
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
