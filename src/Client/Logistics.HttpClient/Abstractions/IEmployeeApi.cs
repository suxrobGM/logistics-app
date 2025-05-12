using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface IEmployeeApi
{
    Task<Result<EmployeeDto>> GetEmployeeAsync(Guid userId);
    Task<PagedResult<EmployeeDto>> GetEmployeesAsync(SearchableQuery query);
    Task<Result> CreateEmployeeAsync(CreateEmployee command);
    Task<Result> UpdateEmployeeAsync(UpdateEmployee command);
    Task<Result> DeleteEmployeeAsync(Guid userId);
}
