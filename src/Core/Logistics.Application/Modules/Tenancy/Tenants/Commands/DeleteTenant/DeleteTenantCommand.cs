
namespace Logistics.Application.Modules.Tenancy.Tenants.Commands;

public class DeleteTenantCommand : ICommand
{
    public Guid Id { get; set; }
}
