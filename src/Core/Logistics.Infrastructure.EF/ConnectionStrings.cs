namespace Logistics.Infrastructure.EF;

internal static class ConnectionStrings
{
    public const string LocalMaster = "Host=localhost; Port=5432; Database=master_logistics; Username=postgres; Password=Test12345#";
    public const string LocalDefaultTenant = "Host=localhost; Port=5432; Database=default_logistics; Username=postgres; Password=Test12345#";
}
