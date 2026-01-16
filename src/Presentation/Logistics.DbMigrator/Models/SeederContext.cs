using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.AspNetCore.Identity;

namespace Logistics.DbMigrator.Models;

/// <summary>
/// Shared context passed to all seeders, containing services and shared state.
/// </summary>
public class SeederContext
{
    public required IServiceProvider ServiceProvider { get; init; }
    public required IConfiguration Configuration { get; init; }
    public required IMasterUnitOfWork MasterUnitOfWork { get; init; }
    public required ITenantUnitOfWork TenantUnitOfWork { get; init; }
    public required UserManager<User> UserManager { get; init; }
    public required RoleManager<AppRole> RoleManager { get; init; }

    /// <summary>
    /// Shared state dictionary for passing data between seeders.
    /// </summary>
    private Dictionary<string, object> SharedState { get; } = [];

    // Strongly-typed accessors for common shared state
    public IList<User>? CreatedUsers
    {
        get => SharedState.TryGetValue(nameof(CreatedUsers), out var val) ? (IList<User>)val : null;
        set => SharedState[nameof(CreatedUsers)] = value!;
    }

    public CompanyEmployees? CreatedEmployees
    {
        get => SharedState.TryGetValue(nameof(CreatedEmployees), out var val) ? (CompanyEmployees)val : null;
        set => SharedState[nameof(CreatedEmployees)] = value!;
    }

    public IList<Customer>? CreatedCustomers
    {
        get => SharedState.TryGetValue(nameof(CreatedCustomers), out var val) ? (IList<Customer>)val : null;
        set => SharedState[nameof(CreatedCustomers)] = value!;
    }

    public IList<Truck>? CreatedTrucks
    {
        get => SharedState.TryGetValue(nameof(CreatedTrucks), out var val) ? (IList<Truck>)val : null;
        set => SharedState[nameof(CreatedTrucks)] = value!;
    }

    public Tenant? DefaultTenant
    {
        get => SharedState.TryGetValue(nameof(DefaultTenant), out var val) ? (Tenant)val : null;
        set => SharedState[nameof(DefaultTenant)] = value!;
    }

    public IList<CustomerUser>? CreatedCustomerUsers
    {
        get => SharedState.TryGetValue(nameof(CreatedCustomerUsers), out var val) ? (IList<CustomerUser>)val : null;
        set => SharedState[nameof(CreatedCustomerUsers)] = value!;
    }
}
