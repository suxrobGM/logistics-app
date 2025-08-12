namespace Logistics.Domain.Exceptions;

[Serializable]
public class SubscriptionExpiredException : Exception
{
    public SubscriptionExpiredException() { }
    public SubscriptionExpiredException(string message) : base(message) { }
    public SubscriptionExpiredException(string message, Exception inner) : base(message, inner) { }
}
