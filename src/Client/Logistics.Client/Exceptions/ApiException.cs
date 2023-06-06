namespace Logistics.Client.Exceptions;

[Serializable]
public class ApiException : Exception
{
    public ApiException() { }
    public ApiException(string message) : base(message) { }
    public ApiException(string message, Exception inner) : base(message, inner) { }
    protected ApiException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
