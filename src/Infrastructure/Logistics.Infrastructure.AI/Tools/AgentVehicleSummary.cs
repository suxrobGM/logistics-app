using Logistics.Shared.Models;

namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// One line per car hauler load, e.g. "2021 Toyota Camry VIN 1HG...; 2018 Ford F-150 (inoperable)",
/// so the agent sees what is on the load without a field per vehicle.
/// </summary>
internal static class AgentVehicleSummary
{
    public static string Describe(IEnumerable<LoadVehicleDto> vehicles) =>
        string.Join("; ", vehicles.Select(Describe));

    private static string Describe(LoadVehicleDto vehicle)
    {
        var name = string.Join(' ', new[] { vehicle.Year?.ToString(), vehicle.Make, vehicle.Model }
            .Where(p => !string.IsNullOrWhiteSpace(p)));
        var parts = new List<string> { string.IsNullOrEmpty(name) ? "Vehicle" : name };

        if (!string.IsNullOrEmpty(vehicle.Vin))
        {
            parts.Add($"VIN {vehicle.Vin}");
        }

        if (!vehicle.IsOperable)
        {
            parts.Add("(inoperable)");
        }

        return string.Join(' ', parts);
    }
}
