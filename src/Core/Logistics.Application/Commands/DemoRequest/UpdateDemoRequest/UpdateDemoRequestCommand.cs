using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public sealed class UpdateDemoRequestCommand : IAppRequest<Result>
{
    public Guid Id { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }
}
