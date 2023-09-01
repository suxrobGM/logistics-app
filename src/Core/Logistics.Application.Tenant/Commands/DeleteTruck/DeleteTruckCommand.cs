namespace Logistics.Application.Tenant.Commands;

public class DeleteTruckCommand : Request<ResponseResult>
{
    public string? Id { get; set; }
}
