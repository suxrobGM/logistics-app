using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Commands;

public class CreateTenantCommand : IAppRequest
{
    public string Name { get; set; } = null!;
    public string? CompanyName { get; set; }
    public string? BillingEmail { get; set; }
    public string? DotNumber { get; set; }
    public Address? CompanyAddress { get; set; }
}
