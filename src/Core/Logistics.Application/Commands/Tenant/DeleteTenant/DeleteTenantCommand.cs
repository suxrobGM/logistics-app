using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteTenantCommand : IAppRequest
{
    public Guid Id { get; set; }
}
