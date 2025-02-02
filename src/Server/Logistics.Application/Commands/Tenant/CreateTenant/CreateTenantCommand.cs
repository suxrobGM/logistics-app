using Logistics.Domain.ValueObjects;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class CreateTenantCommand : IRequest<Result>
{
    public string Name { get; set; } = null!;
    public string? CompanyName { get; set; }
    public Address? CompanyAddress { get; set; }
}
