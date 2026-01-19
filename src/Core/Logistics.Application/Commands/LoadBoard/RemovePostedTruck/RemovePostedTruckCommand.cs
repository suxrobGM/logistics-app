using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class RemovePostedTruckCommand : IAppRequest<Result>
{
    public Guid PostedTruckId { get; set; }
}
