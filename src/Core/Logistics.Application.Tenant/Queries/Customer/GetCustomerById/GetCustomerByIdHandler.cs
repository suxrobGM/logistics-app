using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetCustomerByIdHandler : RequestHandler<GetCustomerByIdQuery, ResponseResult<CustomerDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetCustomerByIdHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult<CustomerDto>> HandleValidated(
        GetCustomerByIdQuery req, CancellationToken cancellationToken)
    {
        var customerEntity = await _tenantUow.Repository<Customer>().GetByIdAsync(req.Id);

        if (customerEntity is null)
        {
            return ResponseResult<CustomerDto>.CreateError($"Could not find a customer with ID {req.Id}");
        }

        var customerDto = customerEntity.ToDto();
        return ResponseResult<CustomerDto>.CreateSuccess(customerDto);
    }
}
