using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DemoRequests.Commands;

public sealed class CreateDemoRequestCommand : ICommand<Result>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? FleetSize { get; set; }
    public string? Message { get; set; }
}
