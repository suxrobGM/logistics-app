namespace Logistics.WebApi.Authorization;

public static class Policies
{
    public static class Employee
    {
        public const string CanRead = "Permissions.Employee.CanRead";
        public const string CanWrite = "Permissions.Employee.CanWrite";
    }
    
    public static class Cargo
    {
        public const string CanRead = "Permissions.Cargo.CanRead";
        public const string CanWrite = "Permissions.Cargo.CanWrite";
    }
    
    public static class Truck
    {
        public const string CanRead = "Permissions.Truck.CanRead";
        public const string CanWrite = "Permissions.Truck.CanWrite";
    }
    
    public static class Tenant
    {
        public const string CanRead = "Permissions.Tenant.CanRead";
        public const string CanWrite = "Permissions.Tenant.CanWrite";
    }
    
    public static class User
    {
        public const string CanRead = "Permissions.User.CanRead";
        public const string CanWrite = "Permissions.User.CanWrite";
    }
}