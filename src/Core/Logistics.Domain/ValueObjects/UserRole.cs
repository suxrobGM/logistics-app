namespace Logistics.Domain.ValueObjects;

public class UserRole : Enumeration
{
    public UserRole(int id, string name) : base(id, name)
    {
    }

    public static readonly UserRole Guest = new(1, "guest");
    public static readonly UserRole Admin = new(3, "admin");

    public static UserRole Get(string name)
    {
        name = name.Trim().ToLower();
        return name switch
        {
            "guest" => Guest,
            "admin" => Admin,
            _ => throw new InvalidOperationException($"Could not found the corresponding enum type for the '{name}'"),
        };
    }
}