namespace Logistics.Application.Tenant.Commands;

public class CreateTruckCommand : Request<ResponseResult>
{
    public string? TruckNumber { get; set; }
    public string[] DriversIds { get; set; }
}
