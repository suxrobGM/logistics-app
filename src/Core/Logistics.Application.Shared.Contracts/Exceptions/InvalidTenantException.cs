namespace Logistics.Application.Shared.Exceptions;

[Serializable]
public class InvalidTenantException : Exception
{
    public InvalidTenantException() { }
    public InvalidTenantException(string message) : base(message) { }
    public InvalidTenantException(string message, Exception inner) : base(message, inner) { }
    protected InvalidTenantException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
