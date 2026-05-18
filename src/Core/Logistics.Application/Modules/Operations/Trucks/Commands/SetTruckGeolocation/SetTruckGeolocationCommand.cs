using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Commands;

public class SetTruckGeolocationCommand : ICommand
{
    public SetTruckGeolocationCommand(TruckGeolocationDto geolocationData)
    {
        GeolocationData = geolocationData;
    }

    public TruckGeolocationDto GeolocationData { get; set; }
}
