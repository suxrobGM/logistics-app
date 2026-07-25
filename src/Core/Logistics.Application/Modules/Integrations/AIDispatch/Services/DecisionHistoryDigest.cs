using System.Text;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Services;

/// <summary>
/// One decision as fed to the learning model. Excludes <c>ToolOutput</c> (largest field, lowest
/// signal) and any resolved entity names.
/// </summary>
/// <param name="Status">Rejected, or Executed for a human-approved action.</param>
/// <param name="RejectionReason">The dispatcher's reason for rejecting. Null for approvals.</param>
internal sealed record DecisionHistoryEntry(
    AIDispatchDecisionStatus Status,
    string? ToolName,
    string? ToolInput,
    string? AgentReasoning,
    string? RejectionReason,
    DateTime CreatedAt);

/// <summary>The assembled prompt text plus the numbers the caller records on the policy row.</summary>
/// <param name="Text">Line-per-decision digest, rejections first.</param>
/// <param name="RejectionCount">Rejections included - the minimum-evidence gate reads this.</param>
/// <param name="LastDecisionAt">Newest <c>CreatedAt</c> across the input, i.e. the new watermark.</param>
internal sealed record DecisionHistoryDigestResult(
    string Text,
    int Count,
    int RejectionCount,
    DateTime? LastDecisionAt);

/// <summary>
/// Turns decision history into bounded prompt text. Pure, so truncation and ordering are testable
/// without an LLM or a database.
/// </summary>
internal static class DecisionHistoryDigest
{
    private const int MaxRejections = 60;
    private const int MaxApprovals = 60;

    /// <summary>Most rows a pass can use, so callers can bound the query that feeds it.</summary>
    public const int MaxDecisions = MaxRejections + MaxApprovals;

    private const int MaxHistoryChars = 12_000;
    private const int MaxToolInputChars = 200;
    private const int MaxReasoningChars = 200;
    private const int MaxRejectionReasonChars = 300;

    public static DecisionHistoryDigestResult Build(IReadOnlyList<DecisionHistoryEntry> decisions)
    {
        // The watermark covers everything examined, including rows the caps drop - otherwise those
        // rows resurface as "new" on every future run.
        DateTime? lastDecisionAt = decisions.Count == 0
            ? null
            : decisions.Max(d => d.CreatedAt);

        // Rejections first: they carry the only behaviour-changing signal, so approvals are the ones
        // squeezed out by the char cap.
        var rejections = decisions
            .Where(d => d.Status == AIDispatchDecisionStatus.Rejected)
            .OrderByDescending(d => d.CreatedAt)
            .Take(MaxRejections)
            .ToList();

        var approvals = decisions
            .Where(d => d.Status != AIDispatchDecisionStatus.Rejected)
            .OrderByDescending(d => d.CreatedAt)
            .Take(MaxApprovals)
            .ToList();

        var builder = new StringBuilder();
        var included = 0;
        var includedRejections = 0;

        foreach (var decision in rejections.Concat(approvals))
        {
            var line = FormatLine(decision);
            if (builder.Length + line.Length > MaxHistoryChars)
            {
                break;
            }

            builder.Append(line);
            included++;
            if (decision.Status == AIDispatchDecisionStatus.Rejected)
            {
                includedRejections++;
            }
        }

        return new DecisionHistoryDigestResult(
            builder.ToString().TrimEnd(),
            included,
            includedRejections,
            lastDecisionAt);
    }

    private static string FormatLine(DecisionHistoryEntry decision)
    {
        var verdict = decision.Status == AIDispatchDecisionStatus.Rejected ? "REJECTED" : "APPROVED";
        var line = new StringBuilder()
            .Append(verdict)
            .Append(" | ").Append(decision.ToolName ?? "unknown")
            .Append(" | ").Append(decision.CreatedAt.ToString("yyyy-MM-dd"));

        if (!string.IsNullOrWhiteSpace(decision.ToolInput))
        {
            line.Append(" | input: ").Append(Truncate(decision.ToolInput, MaxToolInputChars));
        }

        if (!string.IsNullOrWhiteSpace(decision.AgentReasoning))
        {
            line.Append(" | agent: ").Append(Truncate(decision.AgentReasoning, MaxReasoningChars));
        }

        if (!string.IsNullOrWhiteSpace(decision.RejectionReason))
        {
            line.Append(" | reason: ").Append(Truncate(decision.RejectionReason, MaxRejectionReasonChars));
        }

        return line.Append('\n').ToString();
    }

    /// <summary>
    /// Collapses newlines so one decision stays one line, then hard-truncates. These are single
    /// free-text fields, not policy documents, so cutting mid-word is fine.
    /// </summary>
    private static string Truncate(string value, int maxLength)
    {
        // Only pay for a copy when there is something to flatten.
        var flattened = value.AsSpan().IndexOfAny('\r', '\n') < 0
            ? value.Trim()
            : value.ReplaceLineEndings(" ").Trim();

        return flattened.Length <= maxLength ? flattened : flattened[..maxLength];
    }
}
