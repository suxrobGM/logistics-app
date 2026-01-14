namespace Logistics.Shared.Identity.Policies;

public static class AppRolePermissions
{
    public static IEnumerable<string> SuperAdmin => Permission.GetAll();

    public static IEnumerable<string> Admin
    {
        get
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
            list.Add(Permission.Tenant.Manage);
            list.Add(Permission.Tenant.View);
            return list;
        }
    }

    public static IEnumerable<string> Manager
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.Add(Permission.Stat.View);
            list.Add(Permission.Tenant.View);
            return list;
        }
    }

    public static IEnumerable<string> GetBasicPermissions()
    {
        yield return Permission.AppRole.View;
        yield return Permission.TenantRole.View;
        yield return Permission.User.View;
        yield return Permission.Employee.View;
    }
}
