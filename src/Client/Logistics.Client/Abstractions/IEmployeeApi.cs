using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface IEmployeeApi
{
    Task<ResponseResult<EmployeeDto>> GetEmployeeAsync(string userId);
    Task<PagedResponseResult<EmployeeDto>> GetEmployeesAsync(SearchableQuery query);
    Task<ResponseResult> CreateEmployeeAsync(CreateEmployee employee);
    Task<ResponseResult> UpdateEmployeeAsync(UpdateEmployee employee);
    Task<ResponseResult> DeleteEmployeeAsync(string userId);
}
