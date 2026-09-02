using System.Reflection;

namespace Logistics.Shared.Models;

/// <summary>
/// Product name and build version, reported by the version header, the discovery endpoint,
/// and the license heartbeat. The version comes from <c>Directory.Build.props</c>.
/// </summary>
public static class ProductInfo
{
    public const string Name = "LogisticsX";

    public static string Version { get; } =
        typeof(ProductInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? "0.0.0";
}
