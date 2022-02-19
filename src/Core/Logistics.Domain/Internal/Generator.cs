namespace Logistics.Domain;

internal static class Generator
{
    internal static string NewGuid() 
        => Guid.NewGuid().ToString();
}