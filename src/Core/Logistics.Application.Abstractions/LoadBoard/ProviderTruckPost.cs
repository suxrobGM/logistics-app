namespace Logistics.Application.Abstractions.LoadBoard;

/// <summary>
/// A truck post as the load board reports it - just enough to reconcile local posts.
/// </summary>
public sealed record ProviderTruckPost(string ExternalPostId, DateTime? ExpiresAt);
