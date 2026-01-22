using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public sealed class UpdateDemoRequestCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
    public DemoRequestStatus Status { get; set; }
    public string? Notes { get; set; }
}
