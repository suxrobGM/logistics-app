using Logistics.Shared.Identity.Roles;

namespace Logistics.Shared.Identity.Policies;

public static class AppRolePermissions
{
    private static readonly Dictionary<string, Func<IEnumerable<string>>> RolePermissions = new()
    {
        [AppRoles.SuperAdmin] = Permission.GetAll,
        [AppRoles.Admin] = GetAdminPermissions
    };

    public static IEnumerable<string> SuperAdmin => Permission.GetAll();

    public static IEnumerable<string> Admin => GetAdminPermissions();

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

    /// <summary>
    ///     Admins get every feature permission except managing app-level admins,
    ///     which is reserved for <see cref="AppRoles.SuperAdmin" />. New permission
    ///     modules are picked up automatically.
    /// </summary>
    private static IEnumerable<string> GetAdminPermissions()
    {
        return Permission.GetAll().Where(p => p != Permission.AppRole.Manage);
    }
}
