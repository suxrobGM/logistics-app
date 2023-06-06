namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteTruckHandler : RequestHandlerBase<DeleteTruckCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public DeleteTruckHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeleteTruckCommand request, CancellationToken cancellationToken)
    {
        var truck = await _tenantRepository.GetAsync<Truck>(request.Id);
        _tenantRepository.Delete(truck);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
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
