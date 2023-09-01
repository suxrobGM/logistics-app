namespace Logistics.Application.Admin.Commands;

public class DeleteTenantCommand : Request<ResponseResult>
{
    public string? Id { get; set; }
}
