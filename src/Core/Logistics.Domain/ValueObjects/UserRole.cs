namespace Logistics.Domain.ValueObjects;

public class UserRole : Enumeration
{
    public UserRole(int id, string name) : base(id, name)
    {
    }

    public static UserRole Guest = new(1, "guest");
    public static UserRole Manager = new(2, "manager");
    public static UserRole Admin = new(3, "admin");

    public static UserRole Get(string name)
    {
        name = name.Trim().ToLower();
        return name switch
        {
            "guest" => Guest,
            "manager" => Manager,
            "admin" => Admin,
            _ => throw new InvalidOperationException($"Could not found the corresponding enum type for the '{name}'"),
        };
    }
}