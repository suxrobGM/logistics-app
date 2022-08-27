namespace Logistics.Domain.Shared;

public static class TenantRoles
{
    public const string Driver = "tenant.driver";
    public const string Dispatcher = "tenant.dispatcher";
    public const string Manager = "tenant.manager";
    public const string Owner = "tenant.owner";
    
    public static IEnumerable<EnumType> GetValues()
    {
        yield return new EnumType(Driver, "Driver");
        yield return new EnumType(Dispatcher, "Dispatcher");
        yield return new EnumType(Manager, "Manager");
        yield return new EnumType(Owner, "Owner");
    }
}