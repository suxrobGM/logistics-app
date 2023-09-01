namespace Logistics.Application.Tenant.Commands;

public class UpdateTruckCommand : Request<ResponseResult>
{
    public string? Id { get; set; }
    public int? TruckNumber { get; set; }
    public string? DriverId { get; set; }
}
