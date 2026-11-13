using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Queries;

internal sealed class GetEmployeeByIdHandler(ITenantUnitOfWork tenantUow) : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    public async Task<Result<EmployeeDto>> Handle(
        GetEmployeeByIdQuery req, CancellationToken ct)
    {
        var employeeEntity = await tenantUow.Repository<Employee>().GetByIdAsync(req.UserId, ct);

        if (employeeEntity is null)
        {
            return Result<EmployeeDto>.Fail($"Could not find the specified employee with ID {req.UserId}", ErrorCodes.NotFound);
        }

        var employeeDto = employeeEntity.ToDto();
        return Result<EmployeeDto>.Ok(employeeDto);
    }
}
