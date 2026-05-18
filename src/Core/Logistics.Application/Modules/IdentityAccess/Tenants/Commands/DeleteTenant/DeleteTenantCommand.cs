using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

public class DeleteTenantCommand : ICommand
{
    public Guid Id { get; set; }
}
