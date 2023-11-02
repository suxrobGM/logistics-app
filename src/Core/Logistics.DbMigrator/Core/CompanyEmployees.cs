using Logistics.Domain.Entities;

namespace Logistics.DbMigrator.Core;

public record CompanyEmployees(Employee Owner, Employee Manager)
{
    public List<Employee> Dispatchers { get; } = new();
    public List<Employee> Drivers { get; } = new();
    public List<Employee> AllEmployees { get; } = new();
}
