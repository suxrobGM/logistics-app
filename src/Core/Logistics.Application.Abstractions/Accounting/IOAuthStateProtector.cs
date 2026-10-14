namespace Logistics.Application.Abstractions.Accounting;

/// <summary>
///     Protects the OAuth2 <c>state</c> parameter so the anonymous provider callback cannot be forged
///     or replayed later. The state binds the tenant and the user who started the flow, carries a
///     random nonce and expires after <see cref="Lifetime"/>. Backed by ASP.NET Core Data Protection.
/// </summary>
public interface IOAuthStateProtector
{
    /// <summary>How long a state stays valid; the user must finish the provider consent in this window.</summary>
    static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    /// <summary>Produce a tamper-proof, time-limited state string.</summary>
    string Protect(Guid tenantId, Guid userId);

    /// <summary>Recover the state, or null when it is invalid, tampered or expired.</summary>
    OAuthState? TryUnprotect(string state);
}

/// <summary>The tenant and user an OAuth flow was started for.</summary>
public sealed record OAuthState(Guid TenantId, Guid UserId);
