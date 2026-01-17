namespace Logistics.Shared.Identity.Policies;

public static class TenantRolePermissions
{
    public static IEnumerable<string> Owner
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
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
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Eld)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Message)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Invitation)));
            return list;
        }
    }

    public static IEnumerable<string> Manager
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Load)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Truck)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Notification)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Customer)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Payment)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Invoice)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Payroll)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Message)));
            list.Add(Permission.Employee.Manage);
            list.Add(Permission.Stat.View);
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Eld)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Invitation)));
            return list;
        }
    }

    public static IEnumerable<string> Dispatcher
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Load)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Notification)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Customer)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Message)));
            list.Add(Permission.Payment.View);
            list.Add(Permission.Invoice.View);
            list.Add(Permission.Truck.View);
            list.Add(Permission.Stat.View);
            list.Add(Permission.Eld.View);
            return list;
        }
    }

    public static IEnumerable<string> Driver
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Driver)));
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Message)));
            list.Add(Permission.Truck.View);
            list.Add(Permission.Load.View);
            list.Add(Permission.Stat.View);
            return list;
        }
    }

    public static IEnumerable<string> Customer
    {
        get
        {
            var list = new List<string>();
            list.AddRange(GetBasicPermissions());
            list.AddRange(Permission.GeneratePermissions(nameof(Permission.Portal)));
            return list;
        }
    }

    public static IEnumerable<string> GetBasicPermissions() => AppRolePermissions.GetBasicPermissions();
}
