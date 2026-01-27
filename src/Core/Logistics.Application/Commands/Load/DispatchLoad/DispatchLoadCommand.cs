using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DispatchLoadCommand : IAppRequest
{
    public Guid Id { get; set; }
}
