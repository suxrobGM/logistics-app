using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Privacy.Commands;

public class RecordConsentCommand : ICommand<Result<Guid>>
{
    public Guid UserId { get; set; }
    public ConsentType ConsentType { get; set; }
    public bool Granted { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
