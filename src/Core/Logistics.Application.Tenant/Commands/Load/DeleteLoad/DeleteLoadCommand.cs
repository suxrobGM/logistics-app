namespace Logistics.Application.Tenant.Commands;

public class DeleteLoadCommand : Request<ResponseResult>
{
    public string? Id { get; set; }
}
