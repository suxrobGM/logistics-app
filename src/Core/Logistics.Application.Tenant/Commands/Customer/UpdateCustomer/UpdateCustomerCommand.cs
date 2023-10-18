namespace Logistics.Application.Tenant.Commands;

public class UpdateCustomerCommand : Request<ResponseResult>
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
}
