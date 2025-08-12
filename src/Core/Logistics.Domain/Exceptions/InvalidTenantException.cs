namespace Logistics.Domain.Exceptions;

[Serializable]
public class InvalidTenantException : Exception
{
    public InvalidTenantException() { }
    public InvalidTenantException(string message) : base(message) { }
    public InvalidTenantException(string message, Exception inner) : base(message, inner) { }
}
