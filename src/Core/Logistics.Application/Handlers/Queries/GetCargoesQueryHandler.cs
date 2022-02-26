namespace Logistics.Application.Handlers.Queries;

internal sealed class GetCargoesQueryHandler : RequestHandlerBase<GetCargoesQuery, PagedDataResult<CargoDto>>
{
    private readonly IRepository<Cargo> cargoRepository;

    public GetCargoesQueryHandler(IRepository<Cargo> cargoRepository)
    {
        this.cargoRepository = cargoRepository;
    }

    protected override Task<PagedDataResult<CargoDto>> HandleValidated(
        GetCargoesQuery request, 
        CancellationToken cancellationToken)
    {
        var totalItems = cargoRepository.GetQuery().Count();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var items = cargoRepository.GetQuery()
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
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
                AssignedTruckId = i.AssignedTruck.Id,
                Status = i.Status.ToString()
            })
            .ToArray();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

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
