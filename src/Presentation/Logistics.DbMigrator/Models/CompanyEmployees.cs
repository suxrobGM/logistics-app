using Logistics.Domain.Entities;

namespace Logistics.DbMigrator.Models;

public record CompanyEmployees(Employee Owner, Employee Manager)
{
    public List<Employee> Dispatchers { get; } = [];
    public List<Employee> Drivers { get; } = [];
    public List<Employee> AllEmployees { get; } = [];

    /// <summary>An owner-operator has no back office, so the owner dispatches their own work.</summary>
    public List<Employee> DispatcherPool => Dispatchers.Count > 0 ? Dispatchers : [Owner];
}
