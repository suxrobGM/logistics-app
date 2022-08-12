namespace Logistics.Domain.ValueObjects;

public class UserRole : Enumeration
{
    public UserRole(int id, string name) : base(id, name)
    {
    }

    public static readonly UserRole Guest = new(1, "guest");
    public static readonly UserRole Manager = new(2, "main.manager");
    public static readonly UserRole Admin = new(3, "main.admin");

    public static UserRole? Get(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        
        name = name.Trim().ToLower();
        return name switch
        {
            "guest" => Guest,
            "main.manager" => Manager,
            "main.admin" => Admin,
            _ => null
        };
    }
}