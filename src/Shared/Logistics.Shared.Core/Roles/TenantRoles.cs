namespace Logistics.Shared.Roles;

public static class TenantRoles
{
    public const string Owner = "tenant.owner";
    public const string Manager = "tenant.manager";
    public const string Dispatcher = "tenant.dispatcher";
    public const string Driver = "tenant.driver";

    public static IEnumerable<EnumType> GetValues()
    {
        yield return new EnumType(Owner, "Owner");
        yield return new EnumType(Manager, "Manager");
        yield return new EnumType(Dispatcher, "Dispatcher");
        yield return new EnumType(Driver, "Driver");
    }
}
