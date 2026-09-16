using Logistics.Domain.Core;

namespace Logistics.Domain.Entities;

/// <summary>
///     A token signing key for the identity server. Lives in the master DB beside the data
///     protection key ring, because the key ring is what protects <see cref="Data"/>.
///     The <see cref="Entity.Id"/> is also the JWK <c>kid</c> published in the discovery
///     document and in every token header.
/// </summary>
public class SigningKey : Entity, IMasterEntity
{
    public DateTime Created { get; set; }

    public string Algorithm { get; set; } = null!;

    /// <summary>Data-protected RSA parameters. Unreadable without the master DB key ring.</summary>
    public string Data { get; set; } = null!;
}
