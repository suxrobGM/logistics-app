namespace Logistics.Domain.ValueObjects;

public class EmployeeRole : Enumeration
{
    public EmployeeRole(int id, string name) : base(id, name)
    {
    }

    public static readonly EmployeeRole Guest = new(1, "guest");
    public static readonly EmployeeRole Driver = new(2, "driver");
    public static readonly EmployeeRole Dispatcher = new(3, "dispatcher");
    public static readonly EmployeeRole Manager = new(4, "manager");
    public static readonly EmployeeRole Owner = new(5, "owner");

    public static EmployeeRole Get(string name)
    {
        name = name.Trim().ToLower();
        return name switch
        {
            "guest" => Guest,
            "driver" => Driver,
            "dispatcher" => Dispatcher,
            "manager" => Manager,
            "owner" => Owner,
            _ => throw new InvalidOperationException($"Could not found the corresponding enum type for the '{name}'"),
        };
    }
}