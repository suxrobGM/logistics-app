namespace Logistics.Application.Abstractions.Accounting;

/// <summary>
/// The accounting provider rejected the stored tokens: the refresh token expired or was revoked, or the
/// user disconnected the app on the provider's side. Only a new OAuth connection fixes it.
/// </summary>
public sealed class AccountingReconnectRequiredException(string message, Exception? inner = null)
    : Exception(message, inner);
