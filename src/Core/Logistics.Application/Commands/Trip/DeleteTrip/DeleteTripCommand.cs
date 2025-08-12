using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteTripCommand : IAppRequest
{
    public Guid Id { get; set; }
}
