namespace Logistics.Application.Abstractions.Privacy;

/// <summary>
/// Irreversibly replaces a user's PII with placeholder values across the master
/// DB and every tenant DB they belong to. Operational/financial records (loads,
/// invoices, payments, audit logs) keep their referential integrity — only
/// directly-identifying fields (name, email, phone) are scrubbed.
/// Implementations must be idempotent so retries are safe.
/// </summary>
public interface IDataAnonymizer
{
    /// <summary>
    /// Anonymize the given user. Sets <c>User.AnonymizedAt</c> on success.
    /// </summary>
    Task AnonymizeUserAsync(Guid userId, CancellationToken ct = default);
}
