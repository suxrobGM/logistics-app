using Logistics.Domain.Entities;

namespace Logistics.DbMigrator.Models;

public record CompanyEmployees(Employee Owner, Employee Manager)
{
    public List<Employee> Dispatchers { get; } = [];
    public List<Employee> Drivers { get; } = [];
    public List<Employee> AllEmployees { get; } = [];
}
