using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class UpdateUserCommand : IAppRequest
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? TenantId { get; set; }
}
