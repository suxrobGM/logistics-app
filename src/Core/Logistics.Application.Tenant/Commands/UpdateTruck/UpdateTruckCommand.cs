namespace Logistics.Application.Tenant.Commands;

public sealed class UpdateTruckCommand : RequestBase<ResponseResult>
{
    public string? Id { get; set; }
    public int? TruckNumber { get; set; }
    public string? DriverId { get; set; }
}
