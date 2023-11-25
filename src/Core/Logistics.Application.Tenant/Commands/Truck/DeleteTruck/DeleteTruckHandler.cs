namespace Logistics.Application.Tenant.Commands;

internal sealed class DeleteTruckHandler : RequestHandler<DeleteTruckCommand, ResponseResult>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public DeleteTruckHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeleteTruckCommand req, CancellationToken cancellationToken)
    {
        var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(req.Id);
        _tenantUow.Repository<Truck>().Delete(truck);
        await _tenantUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
