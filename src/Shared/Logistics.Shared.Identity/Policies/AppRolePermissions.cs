using Logistics.Shared.Identity.Roles;

namespace Logistics.Shared.Identity.Policies;

public static class AppRolePermissions
{
    private static readonly Dictionary<string, Func<IEnumerable<string>>> RolePermissions = new()
    {
        [AppRoles.SuperAdmin] = Permission.GetAll,
        [AppRoles.Admin] = GetAdminPermissions,
        [AppRoles.Manager] = GetManagerPermissions
    };

    public static IEnumerable<string> SuperAdmin => Permission.GetAll();

    public static IEnumerable<string> Admin => GetAdminPermissions();

    public static IEnumerable<string> Manager => GetManagerPermissions();

    /// <summary>
    ///     Gets permissions for a specific role by name.
    ///     Returns empty if role is not found.
    /// </summary>
    public static IEnumerable<string> GetPermissionsForRole(string roleName)
    {
        return RolePermissions.TryGetValue(roleName, out var permissionsFunc)
            ? permissionsFunc()
            : [];
    }

    /// <summary>
    ///     Registers permissions for a role. Use this to add new roles dynamically.
    /// </summary>
    public static void RegisterRole(string roleName, Func<IEnumerable<string>> permissionsFunc)
    {
        RolePermissions[roleName] = permissionsFunc;
    }

    /// <summary>
    ///     Checks if a role has registered permissions.
    /// </summary>
    public static bool HasRegisteredPermissions(string roleName)
    {
        return RolePermissions.ContainsKey(roleName);
    }

    public static IEnumerable<string> GetBasicPermissions()
    {
        yield return Permission.AppRole.View;
        yield return Permission.TenantRole.View;
        yield return Permission.User.View;
        yield return Permission.Employee.View;
    }

    private static IEnumerable<string> GetAdminPermissions()
    {
        var list = new List<string>();
        list.AddRange(GetBasicPermissions());
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.AppRole)));
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
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.Expense)));
        list.AddRange(Permission.GeneratePermissions(nameof(Permission.BlogPost)));
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
        list.Add(Permission.Stat.View);
        list.Add(Permission.Tenant.View);
        return list;
    }
}
