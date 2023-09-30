using System.Reflection;

namespace Logistics.Shared.Policies;

public static class Permissions
{
    private static readonly IEnumerable<string> Modules; 

    static Permissions()
    {
        Modules = typeof(Permissions)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .Where(type => type is { IsClass: true, IsSealed: true, IsAbstract: true })
            .Select(type => type.Name);
    }
    
    public static class AppRole
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(AppRole)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(AppRole)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(AppRole)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(AppRole)}.Delete";
    }
    
    public static class Employee
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Employee)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Employee)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Employee)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Employee)}.Delete";
    }
    
    public static class Load
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Load)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Load)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Load)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Load)}.Delete";
    }
    
    public static class Stats
    {
        public const string View = $"{nameof(Permissions)}.{nameof(Stats)}.View";
    }
    
    public static class Tenant
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Tenant)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Tenant)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Tenant)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Tenant)}.Delete";
    }
    
    public static class TenantRole
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(TenantRole)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(TenantRole)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(TenantRole)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(TenantRole)}.Delete";
    }
    
    public static class Truck
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(Truck)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(Truck)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(Truck)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(Truck)}.Delete";
    }
    
    public static class User
    {
        public const string Create = $"{nameof(Permissions)}.{nameof(User)}.Create";
        public const string View = $"{nameof(Permissions)}.{nameof(User)}.View";
        public const string Edit = $"{nameof(Permissions)}.{nameof(User)}.Edit";
        public const string Delete = $"{nameof(Permissions)}.{nameof(User)}.Delete";
    }

    public static IEnumerable<string> GetAll()
    {
        var list = new List<string>();

        foreach (var module in Modules)
        {
            list.AddRange(GeneratePermissions(module));
        }
        return list;
    }

    public static IEnumerable<string> GeneratePermissions(string module)
    {
        yield return $"{nameof(Permissions)}.{module}.Create";
        yield return $"{nameof(Permissions)}.{module}.View";
        yield return $"{nameof(Permissions)}.{module}.Edit";
        yield return $"{nameof(Permissions)}.{module}.Delete";
    }
}
