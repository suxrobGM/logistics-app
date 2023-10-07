namespace Logistics.Shared.Policies;

public static class AppRolePermissions
{
    public static IEnumerable<string> SuperAdmin => Permissions.GetAll();

    public static IEnumerable<string> Admin
    {
        get 
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.AppRoles)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Employees)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Loads)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Trucks)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.TenantRoles)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Notifications)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Stats)));
            list.Add(Permissions.Tenants.Create);
            list.Add(Permissions.Tenants.Edit);
            list.Add(Permissions.Tenants.View);
            return list;
        }
    }
    
    public static IEnumerable<string> Manager
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.Add(Permissions.Stats.View);
            list.Add(Permissions.Tenants.View);
            return list;
        }
    }

    public static IEnumerable<string> GetBasicPermissions()
    {
        yield return Permissions.AppRoles.View;
        yield return Permissions.TenantRoles.View;
        yield return Permissions.Users.View;
        yield return Permissions.Employees.View;
    }
}
