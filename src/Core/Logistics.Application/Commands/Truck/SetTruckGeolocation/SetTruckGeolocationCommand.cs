using Logistics.Shared;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class SetTruckGeolocationCommand : IRequest<Result>
{
    public SetTruckGeolocationCommand(TruckGeolocationDto geolocationData)
    {
        GeolocationData = geolocationData;
    }
    
    public TruckGeolocationDto GeolocationData { get; set; }
}
