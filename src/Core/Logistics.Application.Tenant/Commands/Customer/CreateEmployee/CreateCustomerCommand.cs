namespace Logistics.Application.Tenant.Commands;

public class CreateCustomerCommand : Request<ResponseResult>
{
    public string Name { get; set; } = default!;
}
