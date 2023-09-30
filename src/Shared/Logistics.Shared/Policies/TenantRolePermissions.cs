namespace Logistics.Shared.Policies;

public static class TenantRolePermissions
{
    public static IEnumerable<string> Owner
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Employee)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Load)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Truck)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.TenantRole)));
            list.Add(Permissions.Stats.View);
            return list;
        }
    }
    
    public static IEnumerable<string> Manager
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Load)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Truck)));
            list.Add(Permissions.Employee.Create);
            list.Add(Permissions.Employee.Edit);
            list.Add(Permissions.Stats.View);
            return list;
        }
    }
    
    public static IEnumerable<string> Dispatcher
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Load)));
            list.Add(Permissions.Truck.View);
            return list;
        }
    }
    
    public static IEnumerable<string> Driver
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.Add(Permissions.Truck.View);
            list.Add(Permissions.Load.View);
            return list;
        }
    }

    public static IEnumerable<string> GetBasicPermissions() => AppRolePermissions.GetBasicPermissions();
}
