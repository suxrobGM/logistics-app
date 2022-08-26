namespace Logistics.Domain.Shared;

public static class AppRoles
{
    public const string Guest = "app.guest";
    public const string Manager = "app.driver";
    public const string Admin = "app.dispatcher";

    public static IEnumerable<EnumType> GetValues()
    {
        yield return new EnumType(Guest, "Guest");
        yield return new EnumType(Manager, "Manager");
        yield return new EnumType(Admin, "Admin");
    }
}