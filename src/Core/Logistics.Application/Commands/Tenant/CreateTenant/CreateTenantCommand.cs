using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class CreateTenantCommand : IRequest<Result>
{
    public string Name { get; set; } = null!;
    public string? CompanyName { get; set; }
    public string? BillingEmail { get; set; }
    public string? DotNumber { get; set; }
    public Address? CompanyAddress { get; set; }
}
