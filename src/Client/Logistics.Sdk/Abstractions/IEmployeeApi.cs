namespace Logistics.Sdk;

public interface IEmployeeApi
{
    Task<DataResult<EmployeeDto>> GetEmployeeAsync(string id);
    Task<PagedDataResult<EmployeeDto>> GetEmployeesAsync(string searchInput = "", int page = 1, int pageSize = 10);
    Task<DataResult> CreateEmployeeAsync(EmployeeDto employee);
    Task<DataResult> UpdateEmployeeAsync(EmployeeDto employee);
    Task<DataResult> DeleteEmployeeAsync(string id);
}
