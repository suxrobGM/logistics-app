using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetCustomerByIdQuery : Request<ResponseResult<CustomerDto>>
{
    public string? Id { get; set; }
}
