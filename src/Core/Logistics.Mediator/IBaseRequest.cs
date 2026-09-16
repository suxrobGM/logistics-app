namespace Logistics.Mediator;

/// <summary>
/// Non-generic marker for a request. Exists so a request whose response type is not known
/// statically (for example one revived from a background job payload) can still be dispatched.
/// </summary>
public interface IBaseRequest;
