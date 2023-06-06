namespace Logistics.Infrastructure.EF;

internal static class ConnectionStrings
{
    public const string LocalDefaultTenant = "Server=.\\SQLEXPRESS; Database=default_logistics; Uid=LogisticsUser; Pwd=Test12345; TrustServerCertificate=true";
    public const string LocalMain = "Server=.\\SQLEXPRESS; Database=main_logistics; Uid=LogisticsUser; Pwd=Test12345; TrustServerCertificate=true";
}
