namespace Logistics.Shared.Policies;

public static class Permissions
{
    public static class AppRole
    {
        public const string Create = "Permissions.AppRole.Create";
        public const string View = "Permissions.AppRole.View";
        public const string Edit = "Permissions.AppRole.Edit";
        public const string Delete = "Permissions.AppRole.Delete";
    }
    
    public static class Employee
    {
        public const string Create = "Permissions.Employee.Create";
        public const string View = "Permissions.Employee.View";
        public const string Edit = "Permissions.Employee.Edit";
        public const string Delete = "Permissions.Employee.Delete";
    }
    
    public static class Load
    {
        public const string Create = "Permissions.Load.Create";
        public const string View = "Permissions.Load.View";
        public const string Edit = "Permissions.Load.Edit";
        public const string Delete = "Permissions.Load.Delete";
    }
    
    public static class Report
    {
        public const string View = "Permissions.Report.View";
    }
    
    public static class Tenant
    {
        public const string Create = "Permissions.Tenant.Create";
        public const string View = "Permissions.Tenant.View";
        public const string Edit = "Permissions.Tenant.Edit";
        public const string Delete = "Permissions.Tenant.Delete";
    }
    
    public static class TenantRole
    {
        public const string Create = "Permissions.TenantRole.Create";
        public const string View = "Permissions.TenantRole.View";
        public const string Edit = "Permissions.TenantRole.Edit";
        public const string Delete = "Permissions.TenantRole.Delete";
    }
    
    public static class Truck
    {
        public const string Create = "Permissions.Truck.Create";
        public const string View = "Permissions.Truck.View";
        public const string Edit = "Permissions.Truck.Edit";
        public const string Delete = "Permissions.Truck.Delete";
    }
    
    public static class User
    {
        public const string Create = "Permissions.User.Create";
        public const string View = "Permissions.User.View";
        public const string Edit = "Permissions.User.Edit";
        public const string Delete = "Permissions.User.Delete";
    }

    public static IEnumerable<string> GetAll()
    {
        string[] modules = {
            "Employee", 
            "Load", 
            "Tenant", 
            "Truck",
            "User",
            "AppRole",
            "TenantRole"
        };

        var list = new List<string>();

        foreach (var module in modules)
        {
            list.AddRange(GeneratePermissions(module));
        }

        list.Add(Report.View);
        return list;
    }

    public static IEnumerable<string> GeneratePermissions(string module)
    {
        yield return $"Permissions.{module}.Create";
        yield return $"Permissions.{module}.View";
        yield return $"Permissions.{module}.Edit";
        yield return $"Permissions.{module}.Delete";
    }
}