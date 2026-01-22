using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public sealed class CreateDemoRequestCommand : IAppRequest<Result>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? FleetSize { get; set; }
    public string? Message { get; set; }
}
