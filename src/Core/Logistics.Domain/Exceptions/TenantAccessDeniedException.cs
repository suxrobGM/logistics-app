namespace Logistics.Domain.Exceptions;

/// <summary>
/// Thrown when an authenticated caller resolves to a tenant they are not a member of - e.g. a
/// spoofed <c>X-Tenant</c> header naming another company's tenant. Distinct from
/// <see cref="InvalidTenantException"/> (which means "no/unknown tenant") so that behaviours which
/// legitimately tolerate a missing tenant do not also swallow an access-denied, and so it maps to
/// HTTP 403 rather than 500.
/// </summary>
[Serializable]
public class TenantAccessDeniedException : Exception
{
    public TenantAccessDeniedException() { }
    public TenantAccessDeniedException(string message) : base(message) { }
    public TenantAccessDeniedException(string message, Exception inner) : base(message, inner) { }
}
