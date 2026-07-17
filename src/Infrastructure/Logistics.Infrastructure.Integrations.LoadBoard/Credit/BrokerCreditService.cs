using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Credit;

/// <summary>
/// Broker credit lookup: tenant-DB cache (24h TTL) → Demo provider (deterministic pseudo-score)
/// → FMCSA authority status as the free fallback. Credit scores from DAT/Truckstop arrive via
/// search responses and are stamped onto listings by the provider mappers, not fetched here.
/// </summary>
internal sealed class BrokerCreditService(
    ITenantUnitOfWork tenantUow,
    FmcsaClient fmcsaClient,
    ILogger<BrokerCreditService> logger) : IBrokerCreditService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(24);

    public async Task<BrokerCreditDto?> GetBrokerCreditAsync(string? mcNumber, CancellationToken ct = default)
    {
        var normalized = NormalizeMcNumber(mcNumber);
        if (normalized is null)
        {
            return null;
        }

        var repository = tenantUow.Repository<BrokerCreditRecord>();
        var record = await repository.GetAsync(r => r.McNumber == normalized, ct);

        if (record is not null && record.CheckedAt > DateTime.UtcNow - CacheTtl)
        {
            return ToDto(record);
        }

        var fresh = await LookupAsync(normalized, ct);
        if (fresh is null)
        {
            // No source could answer; serve the stale record rather than nothing.
            return record is not null ? ToDto(record) : null;
        }

        if (record is null)
        {
            record = new BrokerCreditRecord
            {
                McNumber = normalized,
                Source = fresh.Source,
                CheckedAt = fresh.CheckedAt
            };
            await repository.AddAsync(record, ct);
        }

        record.CreditScore = fresh.CreditScore;
        record.DaysToPay = fresh.DaysToPay;
        record.AuthorityActive = fresh.AuthorityActive;
        record.Source = fresh.Source;
        record.CheckedAt = fresh.CheckedAt;
        await tenantUow.SaveChangesAsync(ct);

        return fresh;
    }

    private async Task<BrokerCreditDto?> LookupAsync(string mcNumber, CancellationToken ct)
    {
        var hasDemoProvider = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetAsync(c => c.IsActive && c.ProviderType == LoadBoardProviderType.Demo, ct) is not null;

        if (hasDemoProvider)
        {
            return CreateDemoResult(mcNumber);
        }

        var authorityActive = await fmcsaClient.GetAuthorityActiveAsync(mcNumber, ct);
        if (authorityActive is null)
        {
            logger.LogDebug("No broker credit data available for MC {McNumber}", mcNumber);
            return null;
        }

        return new BrokerCreditDto
        {
            McNumber = mcNumber,
            AuthorityActive = authorityActive,
            Source = BrokerCreditSource.Fmcsa,
            CheckedAt = DateTime.UtcNow
        };
    }

    private static BrokerCreditDto CreateDemoResult(string mcNumber)
    {
        var (score, daysToPay, authorityActive) = DemoBrokerCredit.Compute(mcNumber);

        return new BrokerCreditDto
        {
            McNumber = mcNumber,
            CreditScore = score,
            DaysToPay = daysToPay,
            AuthorityActive = authorityActive,
            Source = BrokerCreditSource.Demo,
            CheckedAt = DateTime.UtcNow
        };
    }

    private static string? NormalizeMcNumber(string? mcNumber)
    {
        if (string.IsNullOrWhiteSpace(mcNumber))
        {
            return null;
        }

        var digits = new string(mcNumber.Where(char.IsDigit).ToArray());
        return digits.Length > 0 ? digits : null;
    }

    private static BrokerCreditDto ToDto(BrokerCreditRecord record)
    {
        return new BrokerCreditDto
        {
            McNumber = record.McNumber,
            CreditScore = record.CreditScore,
            DaysToPay = record.DaysToPay,
            AuthorityActive = record.AuthorityActive,
            Source = record.Source,
            CheckedAt = record.CheckedAt
        };
    }
}
