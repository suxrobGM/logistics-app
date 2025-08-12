using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteTruckCommand : IAppRequest
{
    public Guid Id { get; set; }
}
