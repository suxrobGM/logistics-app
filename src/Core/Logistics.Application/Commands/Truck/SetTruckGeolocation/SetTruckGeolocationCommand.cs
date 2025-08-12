using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class SetTruckGeolocationCommand : IAppRequest
{
    public SetTruckGeolocationCommand(TruckGeolocationDto geolocationData)
    {
        GeolocationData = geolocationData;
    }

    public TruckGeolocationDto GeolocationData { get; set; }
}
