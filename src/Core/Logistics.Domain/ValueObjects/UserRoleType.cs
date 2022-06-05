namespace Logistics.Domain.ValueObjects;

public class UserRoleType : Enumeration
{
    public UserRoleType(int id, string name) : base(id, name)
    {
    }

    public static UserRoleType Guest = new(1, "guest");
    public static UserRoleType Driver = new(2, "driver");
    public static UserRoleType Dispatcher = new(3, "dispatcher");
    public static UserRoleType Manager = new(4, "manager");
    public static UserRoleType Owner = new(5, "owner");
    public static UserRoleType Admin = new(6, "admin");

    public static UserRoleType Get(string name)
    {
        name = name.Trim().ToLower();
        return name switch
        {
            "guest" => Guest,
            "driver" => Driver,
            "dispatcher" => Dispatcher,
            "manager" => Manager,
            "owner" => Owner,
            "admin" => Admin,
            _ => throw new InvalidOperationException($"Could not found the corresponding enum type for the '{name}'"),
        };
    }
}