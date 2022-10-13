namespace Logistics.Sdk;

public interface IEmployeeApi
{
    Task<ResponseResult<EmployeeDto>> GetEmployeeAsync(string id);
    Task<PagedResponseResult<EmployeeDto>> GetEmployeesAsync(string searchInput = "", int page = 1, int pageSize = 10);
    Task<ResponseResult> CreateEmployeeAsync(EmployeeDto employee);
    Task<ResponseResult> UpdateEmployeeAsync(EmployeeDto employee);
    Task<ResponseResult> DeleteEmployeeAsync(string id);
}
