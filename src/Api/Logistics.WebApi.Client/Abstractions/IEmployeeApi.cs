namespace Logistics.WebApi.Client;

public interface IEmployeeApi
{
    Task<EmployeeDto> GetEmployeeAsync(string id);
    Task<PagedDataResult<EmployeeDto>> GetEmployeesAsync(string searchInput = "", int page = 1, int pageSize = 10);
    Task<bool> EmployeeExistsAsync(string id);
    Task CreateEmployeeAsync(EmployeeDto employee);
    Task UpdateEmployeeAsync(EmployeeDto employee);
    Task<bool> TryCreateEmployeeAsync(EmployeeDto employee);
    Task DeleteEmployeeAsync(string id);
}
