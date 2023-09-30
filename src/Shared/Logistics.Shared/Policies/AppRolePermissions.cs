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
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.AppRole)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Employee)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Load)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Truck)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.TenantRole)));
            list.Add(Permissions.Stats.View);
            list.Add(Permissions.Tenant.Create);
            list.Add(Permissions.Tenant.Edit);
            list.Add(Permissions.Tenant.View);
            return list;
        }
    }
    
    public static IEnumerable<string> Manager
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.Add(Permissions.Tenant.View);
            return list;
        }
    }

    public static IEnumerable<string> GetBasicPermissions()
    {
        yield return Permissions.AppRole.View;
        yield return Permissions.TenantRole.View;
        yield return Permissions.User.View;
        yield return Permissions.Employee.View;
    }
}
