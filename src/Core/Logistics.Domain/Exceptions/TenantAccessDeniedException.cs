namespace Logistics.Domain.Exceptions;

/// <summary>Thrown when a caller attempts to access another tenant.</summary>
[Serializable]
public class TenantAccessDeniedException : Exception
{
    public TenantAccessDeniedException() { }
    public TenantAccessDeniedException(string message) : base(message) { }
    public TenantAccessDeniedException(string message, Exception inner) : base(message, inner) { }
}
