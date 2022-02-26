namespace Logistics.Application.Handlers.Commands;

internal sealed class DeleteTruckCommandHandler : RequestHandlerBase<DeleteTruckCommand, DataResult>
{
    private readonly IRepository<Truck> truckRepository;

    public DeleteTruckCommandHandler(IRepository<Truck> truckRepository)
    {
        this.truckRepository = truckRepository;
    }

    protected override async Task<DataResult> HandleValidated(DeleteTruckCommand request, CancellationToken cancellationToken)
    {
        truckRepository.Delete(request.Id!);
        await truckRepository.UnitOfWork.CommitAsync();
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
