using Logistics.Shared.Identity.Roles;

namespace Logistics.Shared.Identity.Policies;

public static class TenantRolePermissions
{
    private static readonly Dictionary<string, Func<IEnumerable<string>>> RolePermissions = new()
    {
        [TenantRoles.Owner] = GetOwnerPermissions,
        [TenantRoles.Manager] = GetManagerPermissions,
        [TenantRoles.Dispatcher] = GetDispatcherPermissions,
        [TenantRoles.Driver] = GetDriverPermissions,
        [TenantRoles.Customer] = GetCustomerPermissions,
    };

    public static IEnumerable<string> Owner => GetOwnerPermissions();
    public static IEnumerable<string> Manager => GetManagerPermissions();
    public static IEnumerable<string> Dispatcher => GetDispatcherPermissions();
    public static IEnumerable<string> Driver => GetDriverPermissions();
    public static IEnumerable<string> Customer => GetCustomerPermissions();

    /// <summary>
    /// Gets permissions for a specific role by name.
    /// Returns empty if role is not found.
    /// </summary>
    public static IEnumerable<string> GetPermissionsForRole(string roleName)
    {
        return RolePermissions.TryGetValue(roleName, out var permissionsFunc)
            ? permissionsFunc()
            : [];
    }

    /// <summary>
    /// Registers permissions for a role. Use this to add new roles dynamically.
    /// </summary>
    public static void RegisterRole(string roleName, Func<IEnumerable<string>> permissionsFunc)
    {
        RolePermissions[roleName] = permissionsFunc;
    }

    /// <summary>
    /// Checks if a role has registered permissions.
    /// </summary>
    public static bool HasRegisteredPermissions(string roleName)
    {
        return RolePermissions.ContainsKey(roleName);
    }

    public static IEnumerable<string> GetBasicPermissions() => AppRolePermissions.GetBasicPermissions();

    private static IEnumerable<string> GetOwnerPermissions()
    {
        var list = new List<string>();
        list.AddRange(GetBasicPermissions());
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Employee)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Load)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Truck)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.TenantRole)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Notification)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Stat)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Customer)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Payment)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Invoice)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Payroll)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Eld)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Message)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Invitation)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Expense)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.LoadBoard)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Tenant)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Dvir)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Safety)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Maintenance)));
        return list;
    }

    private static IEnumerable<string> GetManagerPermissions()
    {
        var list = new List<string>();
        list.AddRange(GetBasicPermissions());
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Load)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Truck)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Notification)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Customer)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Payment)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Invoice)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Payroll)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Message)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Expense)));
        list.Add(Permission.Employee.Manage);
        list.Add(Permission.Stat.View);
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Eld)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Invitation)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.LoadBoard)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Dvir)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Safety)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Maintenance)));
        return list;
    }

    private static IEnumerable<string> GetDispatcherPermissions()
    {
        var list = new List<string>();
        list.AddRange(GetBasicPermissions());
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Load)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Notification)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Customer)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Message)));
        list.Add(Permission.Payment.View);
        list.Add(Permission.Invoice.View);
        list.Add(Permission.Truck.View);
        list.Add(Permission.Stat.View);
        list.Add(Permission.Eld.View);
        list.Add(Permission.Expense.View);
        list.Add(Permission.Expense.Manage);
        list.Add(Permission.LoadBoard.View);
        list.Add(Permission.LoadBoard.Search);
        list.Add(Permission.LoadBoard.Book);
        list.Add(Permission.LoadBoard.Post);
        return list;
    }

    private static IEnumerable<string> GetDriverPermissions()
    {
        var list = new List<string>();
        list.AddRange(GetBasicPermissions());
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Driver)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Message)));
        list.Add(Permission.Truck.View);
        list.Add(Permission.Load.View);
        list.Add(Permission.Stat.View);
        return list;
    }

    private static IEnumerable<string> GetCustomerPermissions()
    {
        var list = new List<string>();
        list.AddRange(GetBasicPermissions());
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Portal)));
        return list;
    }
}
