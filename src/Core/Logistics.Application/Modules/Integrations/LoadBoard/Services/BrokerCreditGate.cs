using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Services;

/// <summary>
/// The one place that decides whether a broker's credit is good enough to commit to. Shared by
/// booking and rate negotiation so both refuse the same brokers for the same reasons.
/// </summary>
internal static class BrokerCreditGate
{
    /// <summary>
    /// Fetches credit and stamps it on the listing either way (an audit trail of what was known at
    /// the time), then fails when the broker is unacceptable and the caller has no override.
    /// </summary>
    public static async Task<Result> EvaluateAsync(
        ITenantUnitOfWork tenantUow,
        IBrokerCreditService brokerCreditService,
        LoadBoardListing listing,
        bool overrideCheck,
        CancellationToken ct)
    {
        var credit = await brokerCreditService.GetBrokerCreditAsync(listing.BrokerMcNumber, ct);
        if (credit is not null)
        {
            listing.BrokerCreditScore = credit.CreditScore ?? listing.BrokerCreditScore;
            listing.BrokerDaysToPay = credit.DaysToPay ?? listing.BrokerDaysToPay;
            listing.BrokerCreditCheckedAt = credit.CheckedAt;
            await tenantUow.SaveChangesAsync(ct);
        }

        if (overrideCheck)
        {
            return Result.Ok();
        }

        if (credit?.AuthorityActive == false)
        {
            return Result.Fail(
                $"Broker '{listing.BrokerName}' (MC {listing.BrokerMcNumber}) has inactive FMCSA operating authority.",
                ErrorCodes.BrokerCreditBelowThreshold);
        }

        var minScore = tenantUow.GetCurrentTenant().Settings.MinBrokerCreditScore;
        var effectiveScore = credit?.CreditScore ?? listing.BrokerCreditScore;

        // A missing score never blocks; only a known score below the tenant threshold does.
        if (minScore.HasValue && effectiveScore < minScore)
        {
            return Result.Fail(
                $"Broker '{listing.BrokerName}' (MC {listing.BrokerMcNumber}) credit score {effectiveScore} is below your minimum of {minScore}.",
                ErrorCodes.BrokerCreditBelowThreshold);
        }

        return Result.Ok();
    }
}
