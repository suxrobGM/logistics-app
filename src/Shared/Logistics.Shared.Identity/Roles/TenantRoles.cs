namespace Logistics.Shared.Identity.Roles;

public static class TenantRoles
{
    public const string Owner = "tenant.owner";
    public const string Manager = "tenant.manager";
    public const string Dispatcher = "tenant.dispatcher";
    public const string Driver = "tenant.driver";

    public static IEnumerable<EnumType<string>> GetValues()
    {
        yield return new EnumType<string>(Owner, "Owner");
        yield return new EnumType<string>(Manager, "Manager");
        yield return new EnumType<string>(Dispatcher, "Dispatcher");
        yield return new EnumType<string>(Driver, "Driver");
    }
}
