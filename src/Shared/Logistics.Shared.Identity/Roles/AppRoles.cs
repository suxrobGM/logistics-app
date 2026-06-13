namespace Logistics.Shared.Identity.Roles;

public static class AppRoles
{
    public const string SuperAdmin = "app.superadmin";
    public const string Admin = "app.admin";

    public static IEnumerable<EnumType<string>> GetValues()
    {
        yield return new EnumType<string>(SuperAdmin, "Super Admin");
        yield return new EnumType<string>(Admin, "Admin");
    }
}
