using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

public class RecordConsentCommand : IAppRequest<Result<Guid>>
{
    /// <summary>
    /// Set when the caller is authenticated; otherwise null.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Browser-scoped GUID stored alongside the consent cookie. Required when UserId is null.
    /// </summary>
    public Guid? AnonymousId { get; set; }

    public ConsentType ConsentType { get; set; }
    public bool Granted { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
