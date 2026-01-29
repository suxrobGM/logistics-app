using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class CreateEmergencyContactCommand : IAppRequest<Result<EmergencyContactDto>>
{
    public Guid? EmployeeId { get; set; }
    public required string Name { get; set; }
    public required EmergencyContactType ContactType { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int Priority { get; set; } = 1;
}
