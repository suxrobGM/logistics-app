using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteLoadCommand : IAppRequest
{
    public Guid Id { get; set; }
}
