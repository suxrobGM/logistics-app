namespace Logistics.Shared.Identity.Policies;

public static class TenantRolePermissions
{
    public static IEnumerable<string> Owner
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Employees)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Loads)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Trucks)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.TenantRoles)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Notifications)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Stats)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Customers)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Payments)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Invoices)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Payrolls)));
            return list;
        }
    }

    public static IEnumerable<string> Manager
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Loads)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Trucks)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Notifications)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Customers)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Payments)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Invoices)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Payrolls)));
            list.Add(Permissions.Employees.Create);
            list.Add(Permissions.Employees.Edit);
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
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Loads)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Notifications)));
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Customers)));
            list.Add(Permissions.Payments.View);
            list.Add(Permissions.Invoices.View);
            list.Add(Permissions.Trucks.View);
            list.Add(Permissions.Stats.View);
            return list;
        }
    }

    public static IEnumerable<string> Driver
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permissions.GeneratePermissions(nameof(Permissions.Drivers)));
            list.Add(Permissions.Trucks.View);
            list.Add(Permissions.Loads.View);
            list.Add(Permissions.Stats.View);
            return list;
        }
    }

    public static IEnumerable<string> GetBasicPermissions() => AppRolePermissions.GetBasicPermissions();
}
