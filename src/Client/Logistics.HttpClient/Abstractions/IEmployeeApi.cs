using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface IEmployeeApi
{
    Task<Result<EmployeeDto>> GetEmployeeAsync(Guid userId);
    Task<PagedResult<EmployeeDto>> GetEmployeesAsync(SearchableQuery query);
    Task<Result> CreateEmployeeAsync(CreateEmployeeCommand command);
    Task<Result> UpdateEmployeeAsync(UpdateEmployeeCommand command);
    Task<Result> DeleteEmployeeAsync(Guid userId);
}
