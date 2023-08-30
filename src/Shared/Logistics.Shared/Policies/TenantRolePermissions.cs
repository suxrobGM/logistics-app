namespace Logistics.Shared.Policies;

public static class TenantRolePermissions
{
    public static IEnumerable<string> Owner
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permissions.GeneratePermissions("Employee"));
            list.AddRange(Permissions.GeneratePermissions("Load"));
            list.AddRange(Permissions.GeneratePermissions("Truck"));
            list.AddRange(Permissions.GeneratePermissions("TenantRole"));
            list.Add(Permissions.Report.View);
            return list;
        }
    }
    
    public static IEnumerable<string> Manager
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permissions.GeneratePermissions("Load"));
            list.AddRange(Permissions.GeneratePermissions("Truck"));
            list.Add(Permissions.Employee.Create);
            list.Add(Permissions.Employee.Edit);
            list.Add(Permissions.Report.View);
            return list;
        }
    }
    
    public static IEnumerable<string> Dispatcher
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permissions.GeneratePermissions("Load"));
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
            return list;
        }
    }

    public static IEnumerable<string> GetBasicPermissions() => AppRolePermissions.GetBasicPermissions();
}