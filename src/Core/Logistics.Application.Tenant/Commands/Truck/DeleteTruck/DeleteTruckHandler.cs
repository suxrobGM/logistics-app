namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteTruckHandler : RequestHandler<DeleteTruckCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public DeleteTruckHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeleteTruckCommand req, CancellationToken cancellationToken)
    {
        var truck = await _tenantRepository.GetAsync<Truck>(req.Id);
        _tenantRepository.Delete(truck);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
