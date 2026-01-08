using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface IEmployeeApi
{
    Task<EmployeeDto?> GetEmployeeAsync(Guid userId);
    Task<PagedResponse<EmployeeDto>?> GetEmployeesAsync(SearchableQuery query);
    Task<bool> CreateEmployeeAsync(CreateEmployeeCommand command);
    Task<bool> UpdateEmployeeAsync(UpdateEmployeeCommand command);
    Task<bool> DeleteEmployeeAsync(Guid userId);
}
