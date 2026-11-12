using Logistics.Domain.Entities;
using Logistics.Domain.Primitives;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Services;

internal static class LoadVehicleInputExtensions
{
    extension(IEnumerable<LoadVehicleInput> inputs)
    {
        /// <summary>
        /// Builds the load's vehicle list, keeping each sent id so condition reports stay attached.
        /// </summary>
        public List<LoadVehicle> ToLoadVehicles() =>
            [.. inputs.Select(v => new LoadVehicle
            {
                Id = v.Id ?? Guid.CreateVersion7(),
                Vin = VinFormat.Normalize(v.Vin),
                Year = v.Year,
                Make = Clean(v.Make),
                Model = Clean(v.Model),
                Color = Clean(v.Color),
                LotNumber = Clean(v.LotNumber),
                IsOperable = v.IsOperable,
                Notes = Clean(v.Notes),
                Rate = v.Rate
            })];
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
