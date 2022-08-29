namespace Logistics.Application.Handlers.Commands;

internal sealed class DeleteTruckHandler : RequestHandlerBase<DeleteTruckCommand, DataResult>
{
    private readonly ITenantRepository _tenantRepository;

    public DeleteTruckHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        DeleteTruckCommand request, CancellationToken cancellationToken)
    {
        var truck = await _tenantRepository.GetAsync<Truck>(request.Id);
        _tenantRepository.Delete(truck);
        await _tenantRepository.UnitOfWork.CommitAsync();
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
