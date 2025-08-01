using System.Reflection;

namespace Logistics.Shared.Identity.Policies;

public static class Permissions
{
    private static readonly Dictionary<string, IEnumerable<string>> Modules;

    static Permissions()
    {
        Modules = typeof(Permissions)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .Where(type => type is { IsClass: true, IsSealed: true, IsAbstract: true })
            .ToDictionary(
                type => type.Name, 
                type => type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(field => field is { IsLiteral: true, IsInitOnly: false })
                    .Select(field => (string)field.GetValue(null)!)
            );
    }
    
    public static class AppRoles
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(AppRoles)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(AppRoles)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(AppRoles)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(AppRoles)}.Delete";
    }
    
    public static class Employees
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Employees)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Employees)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Employees)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Employees)}.Delete";
    }
    
    public static class Customers
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Customers)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Customers)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Customers)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Customers)}.Delete";
    }
    
    public static class Drivers
    {
        public const string View = $"{nameof(Permissions)}.{nameof(Drivers)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Drivers)}.Edit";
    }
    
    public static class Notifications
    {
        public const string View = $"{nameof(Permissions)}.{nameof(Notifications)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Notifications)}.Edit";
    }
    
    public static class Loads
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Loads)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Loads)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Loads)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Loads)}.Delete";
    }
    
    public static class Stats
    {
        public const string View = $"{nameof(Permissions)}.{nameof(Stats)}.View";
    }
    
    public static class Tenants
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Tenants)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Tenants)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Tenants)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Tenants)}.Delete";
    }
    
    public static class TenantRoles
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(TenantRoles)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(TenantRoles)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(TenantRoles)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(TenantRoles)}.Delete";
    }
    
    public static class Trucks
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Trucks)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Trucks)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Trucks)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Trucks)}.Delete";
    }
    
    public static class Users
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Users)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Users)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Users)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Users)}.Delete";
    }
    
    public static class Payments
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Payments)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Payments)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Payments)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Payments)}.Delete";
    }
    
    public static class Invoices
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Invoices)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Invoices)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Invoices)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Invoices)}.Delete";
    }
    
    public static class Payrolls
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Payrolls)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Payrolls)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Payrolls)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Payrolls)}.Delete";
    }

    public static IEnumerable<string> GetAll()
    {
        var list = new List<string>();

        foreach (var module in Modules.Keys)
        {
            list.AddRange(GeneratePermissions(module));
        }
        return list;
    }

    public static IEnumerable<string> GeneratePermissions(string module)
    {
        return Modules[module];
    }
}
