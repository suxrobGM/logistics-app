using System.Reflection;

namespace Logistics.Shared.Identity.Policies;

public static class Permission
{
    private static readonly Dictionary<string, IEnumerable<string>> Modules;

    static Permission()
    {
        Modules = typeof(Permission)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .Where(type => type is { IsClass: true, IsSealed: true, IsAbstract: true })
            .ToDictionary(
                type => type.Name,
                type => type.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Where(field => field is { IsLiteral: true, IsInitOnly: false })
                    .Select(field => (string)field.GetValue(null)!)
            );
    }

    public static class AppRole
    {
        public const string View = $"{nameof(Permission)}.{nameof(AppRole)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(AppRole)}.Manage";
    }

    public static class Employee
    {
        public const string View = $"{nameof(Permission)}.{nameof(Employee)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Employee)}.Manage";
    }

    public static class Customer
    {
        public const string View = $"{nameof(Permission)}.{nameof(Customer)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Customer)}.Manage";
    }

    public static class Driver
    {
        public const string View = $"{nameof(Permission)}.{nameof(Driver)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Driver)}.Manage";
    }

    public static class Notification
    {
        public const string View = $"{nameof(Permission)}.{nameof(Notification)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Notification)}.Manage";
    }

    public static class Load
    {
        public const string View = $"{nameof(Permission)}.{nameof(Load)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Load)}.Manage";
    }

    public static class Stat
    {
        public const string View = $"{nameof(Permission)}.{nameof(Stat)}.View";
    }

    public static class Tenant
    {
        public const string View = $"{nameof(Permission)}.{nameof(Tenant)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Tenant)}.Manage";
    }

    public static class TenantRole
    {
        public const string View = $"{nameof(Permission)}.{nameof(TenantRole)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(TenantRole)}.Manage";
    }

    public static class Truck
    {
        public const string View = $"{nameof(Permission)}.{nameof(Truck)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Truck)}.Manage";
    }

    public static class User
    {
        public const string View = $"{nameof(Permission)}.{nameof(User)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(User)}.Manage";
    }

    public static class Payment
    {
        public const string View = $"{nameof(Permission)}.{nameof(Payment)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Payment)}.Manage";
    }

    public static class Invoice
    {
        public const string View = $"{nameof(Permission)}.{nameof(Invoice)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Invoice)}.Manage";
    }

    public static class Payroll
    {
        public const string View = $"{nameof(Permission)}.{nameof(Payroll)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Payroll)}.Manage";
        public const string Approve = $"{nameof(Permission)}.{nameof(Payroll)}.Approve";
    }

    public static class Eld
    {
        public const string View = $"{nameof(Permission)}.{nameof(Eld)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Eld)}.Manage";
        public const string Sync = $"{nameof(Permission)}.{nameof(Eld)}.Sync";
    }

    public static class Message
    {
        public const string View = $"{nameof(Permission)}.{nameof(Message)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Message)}.Manage";
    }

    public static class Invitation
    {
        public const string View = $"{nameof(Permission)}.{nameof(Invitation)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Invitation)}.Manage";
    }

    public static class Portal
    {
        public const string Access = $"{nameof(Permission)}.{nameof(Portal)}.Access";
        public const string ViewLoads = $"{nameof(Permission)}.{nameof(Portal)}.ViewLoads";
        public const string ViewInvoices = $"{nameof(Permission)}.{nameof(Portal)}.ViewInvoices";
        public const string ViewDocuments = $"{nameof(Permission)}.{nameof(Portal)}.ViewDocuments";
    }

    public static class Expense
    {
        public const string View = $"{nameof(Permission)}.{nameof(Expense)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Expense)}.Manage";
        public const string Approve = $"{nameof(Permission)}.{nameof(Expense)}.Approve";
    }

    public static class LoadBoard
    {
        public const string View = $"{nameof(Permission)}.{nameof(LoadBoard)}.View";
        public const string Search = $"{nameof(Permission)}.{nameof(LoadBoard)}.Search";
        public const string Book = $"{nameof(Permission)}.{nameof(LoadBoard)}.Book";
        public const string Post = $"{nameof(Permission)}.{nameof(LoadBoard)}.Post";
        public const string Manage = $"{nameof(Permission)}.{nameof(LoadBoard)}.Manage";
    }

    public static class BlogPost
    {
        public const string View = $"{nameof(Permission)}.{nameof(BlogPost)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(BlogPost)}.Manage";
    }

    public static class Dvir
    {
        public const string View = $"{nameof(Permission)}.{nameof(Dvir)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Dvir)}.Manage";
        public const string Review = $"{nameof(Permission)}.{nameof(Dvir)}.Review";
    }

    public static class Safety
    {
        public const string View = $"{nameof(Permission)}.{nameof(Safety)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Safety)}.Manage";
    }

    public static class Maintenance
    {
        public const string View = $"{nameof(Permission)}.{nameof(Maintenance)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Maintenance)}.Manage";
    }

    public static class Certification
    {
        public const string View = $"{nameof(Permission)}.{nameof(Certification)}.View";
        public const string Manage = $"{nameof(Permission)}.{nameof(Certification)}.Manage";
        public const string Verify = $"{nameof(Permission)}.{nameof(Certification)}.Verify";
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
