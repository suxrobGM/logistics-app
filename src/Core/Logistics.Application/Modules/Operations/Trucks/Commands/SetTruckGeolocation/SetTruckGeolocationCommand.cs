using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

public class SetTruckGeolocationCommand(TruckGeolocationDto geolocationData) : ICommand
{
    public TruckGeolocationDto GeolocationData { get; set; } = geolocationData;
}
