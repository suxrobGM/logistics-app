namespace Logistics.Infrastructure;

/// <summary>
///     Local connection strings used for development (e.g., creating migrations).
/// </summary>
internal static class ConnectionStrings
{
    public const string LocalMaster =
        "Host=localhost; Port=5432; Database=master_logistics; Username=postgres; Password=Hamada1020$";

    public const string LocalDefaultTenant =
        "Host=localhost; Port=5432; Database=default_logistics; Username=postgres; Password=Hamada1020$";
}
