namespace Logistics.Application.Tenant.Commands;

public class DeleteCustomerCommand : Request<ResponseResult>
{
    public string Id { get; set; } = default!;
}
