using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteTenantCommand : ICommand
{
    public Guid Id { get; set; }
}
