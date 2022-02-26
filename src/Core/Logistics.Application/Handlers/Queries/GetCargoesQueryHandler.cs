namespace Logistics.Application.Handlers.Queries;

internal sealed class GetCargoesQueryHandler : GetPagedQueryHandlerBase<GetCargoesQuery, CargoDto>
{
    private readonly IRepository<Cargo> cargoRepository;

    public GetCargoesQueryHandler(IRepository<Cargo> cargoRepository)
    {
        this.cargoRepository = cargoRepository;
    }

    protected override Task<PagedDataResult<CargoDto>> HandleValidated(
        GetPagedQueryBase<CargoDto> request, 
        CancellationToken cancellationToken)
    {
        var totalItems = cargoRepository.GetQuery().Count();

        var items = cargoRepository.GetQuery()
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new CargoDto
            {
                Source = i.Source,
                Destination = i.Destination,
                PricePerMile = i.PricePerMile,
                TotalTripMiles = i.TotalTripMiles,
                PickUpDate = i.PickUpDate,
                IsCompleted = i.IsCompleted,
                AssignedDispatcherId = i.AssignedDispatcherId,
                AssignedTruckId = i.AssignedTruck.Id,
                Status = i.Status.ToString()
            })
            .ToArray();

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<CargoDto>(items, totalItems, totalPages));
    }
}
