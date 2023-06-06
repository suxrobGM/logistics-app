using Logistics.Client.Models;

namespace Logistics.Client.Abstractions;

public interface IEmployeeApi
{
    Task<ResponseResult<Employee>> GetEmployeeAsync(string id);
    Task<PagedResponseResult<Employee>> GetEmployeesAsync(SearchableQuery query);
    Task<ResponseResult> CreateEmployeeAsync(CreateEmployee employee);
    Task<ResponseResult> UpdateEmployeeAsync(UpdateEmployee employee);
    Task<ResponseResult> DeleteEmployeeAsync(string id);
}
