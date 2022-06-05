namespace Logistics.Application.Handlers.Commands;

internal sealed class DeleteTruckHandler : RequestHandlerBase<DeleteTruckCommand, DataResult>
{
    private readonly ITenantRepository<Truck> _truckRepository;

    public DeleteTruckHandler(ITenantRepository<Truck> truckRepository)
    {
        _truckRepository = truckRepository;
    }

    protected override async Task<DataResult> HandleValidated(DeleteTruckCommand request, CancellationToken cancellationToken)
    {
        _truckRepository.Delete(request.Id!);
        await _truckRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(DeleteTruckCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
