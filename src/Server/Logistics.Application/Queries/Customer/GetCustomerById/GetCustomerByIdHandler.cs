using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetCustomerByIdHandler : RequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetCustomerByIdHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<CustomerDto>> HandleValidated(
        GetCustomerByIdQuery req, CancellationToken cancellationToken)
    {
        var customerEntity = await _tenantUow.Repository<Customer>().GetByIdAsync(req.Id);

        if (customerEntity is null)
        {
            return Result<CustomerDto>.Fail($"Could not find a customer with ID {req.Id}");
        }

        var customerDto = customerEntity.ToDto();
        return Result<CustomerDto>.Succeed(customerDto);
    }
}
