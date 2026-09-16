namespace Logistics.Mediator;

/// <summary>
/// Non-generic request marker, so a request whose response type is not known statically (one
/// revived from a background job payload, say) can still be dispatched.
/// </summary>
public interface IBaseRequest;
