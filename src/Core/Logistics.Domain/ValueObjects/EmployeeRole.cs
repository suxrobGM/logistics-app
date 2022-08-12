namespace Logistics.Domain.ValueObjects;

public class EmployeeRole : Enumeration
{
    public EmployeeRole(int id, string name) : base(id, name)
    {
    }

    public static readonly EmployeeRole Guest = new(1, "guest");
    public static readonly EmployeeRole Driver = new(2, "tenant.driver");
    public static readonly EmployeeRole Dispatcher = new(3, "tenant.dispatcher");
    public static readonly EmployeeRole Manager = new(4, "tenant.manager");
    public static readonly EmployeeRole Owner = new(5, "tenant.owner");

    public static EmployeeRole? Get(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        
        name = name.Trim().ToLower();
        return name switch
        {
            "guest" => Guest,
            "tenant.driver" => Driver,
            "tenant.dispatcher" => Dispatcher,
            "tenant.manager" => Manager,
            "tenant.owner" => Owner,
            _ => null,
        };
    }
}