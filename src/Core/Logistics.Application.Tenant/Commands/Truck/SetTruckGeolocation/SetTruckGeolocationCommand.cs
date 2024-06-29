using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class SetTruckGeolocationCommand : IRequest<Result>
{
    public SetTruckGeolocationCommand(TruckGeolocationDto geolocationData)
    {
        GeolocationData = geolocationData;
    }
    
    public TruckGeolocationDto GeolocationData { get; set; }
}
