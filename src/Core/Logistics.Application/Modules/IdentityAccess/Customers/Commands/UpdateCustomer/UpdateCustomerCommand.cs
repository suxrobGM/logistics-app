using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.IdentityAccess.Customers.Commands;

public class UpdateCustomerCommand : ICommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public Address? Address { get; set; }
    public CustomerStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? TaxId { get; set; }
    public bool IsVatExempt { get; set; }
}
