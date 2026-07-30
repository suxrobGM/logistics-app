using Logistics.Application.Modules.Integrations.AIDispatch.Services;
using Logistics.Domain.Primitives.Enums;
using Xunit;

namespace Logistics.Application.Tests.AIDispatch;

public class DecisionHistoryDigestTests
{
    [Fact]
    public void Build_Empty_ReturnsZeroesAndNoWatermark()
    {
        var result = DecisionHistoryDigest.Build([]);

        Assert.Equal(0, result.Count);
        Assert.Equal(0, result.RejectionCount);
        Assert.Null(result.LastDecisionAt);
        Assert.Empty(result.Text);
    }

    [Fact]
    public void Build_LabelsVerdictsAndCountsRejections()
    {
        var entries = new List<DecisionHistoryEntry>
        {
            Entry(AgentDecisionStatus.Rejected, rejectionReason: "Deadhead too far"),
            Entry(AgentDecisionStatus.Executed),
            Entry(AgentDecisionStatus.Rejected, rejectionReason: "Rate too low")
        };

        var result = DecisionHistoryDigest.Build(entries);

        Assert.Equal(3, result.Count);
        Assert.Equal(2, result.RejectionCount);
        Assert.Contains("REJECTED", result.Text);
        Assert.Contains("APPROVED", result.Text);
        Assert.Contains("Deadhead too far", result.Text);
    }

    /// <summary>Rejections carry the only behaviour-changing signal, so the caps drop approvals first.</summary>
    [Fact]
    public void Build_ListsRejectionsBeforeApprovals()
    {
        var entries = new List<DecisionHistoryEntry>
        {
            Entry(AgentDecisionStatus.Executed, toolName: "approved_tool", minutesAgo: 1),
            Entry(AgentDecisionStatus.Rejected, toolName: "rejected_tool", minutesAgo: 100)
        };

        var result = DecisionHistoryDigest.Build(entries);

        Assert.True(
            result.Text.IndexOf("rejected_tool", StringComparison.Ordinal) <
            result.Text.IndexOf("approved_tool", StringComparison.Ordinal));
    }

    /// <summary>
    /// The watermark covers everything examined, including dropped rows - otherwise they look "new"
    /// forever and the job never converges.
    /// </summary>
    [Fact]
    public void Build_WatermarkCoversAllInputEvenBeyondCaps()
    {
        var newest = DateTime.UtcNow;
        var entries = Enumerable.Range(0, 200)
            .Select(i => Entry(AgentDecisionStatus.Rejected, minutesAgo: i))
            .ToList();

        var result = DecisionHistoryDigest.Build(entries);

        Assert.True(result.Count < 200, "The caps must actually drop rows for this test to mean anything");
        Assert.Equal(newest.Date, result.LastDecisionAt!.Value.Date);
        Assert.True(result.LastDecisionAt >= newest.AddSeconds(-5));
    }

    [Fact]
    public void Build_CapsRejectionsAtSixty()
    {
        var entries = Enumerable.Range(0, 100)
            .Select(i => Entry(AgentDecisionStatus.Rejected, minutesAgo: i))
            .ToList();

        var result = DecisionHistoryDigest.Build(entries);

        Assert.True(result.RejectionCount <= 60);
    }

    /// <summary>Tool output is the biggest field and the least useful - it must never be sent.</summary>
    [Fact]
    public void Build_NeverEmitsToolOutput()
    {
        var entries = Enumerable.Range(0, 5)
            .Select(_ => Entry(AgentDecisionStatus.Rejected, rejectionReason: "nope"))
            .ToList();

        var result = DecisionHistoryDigest.Build(entries);

        // DecisionHistoryEntry has no ToolOutput member - assert the shape stays that way.
        Assert.DoesNotContain("tool_output", result.Text);
        Assert.DoesNotContain("output:", result.Text);
    }

    [Fact]
    public void Build_TruncatesLongFieldsAndKeepsOneLinePerDecision()
    {
        var entries = new List<DecisionHistoryEntry>
        {
            Entry(
                AgentDecisionStatus.Rejected,
                toolInput: new string('i', 500),
                reasoning: new string('r', 500),
                rejectionReason: new string('x', 900))
        };

        var result = DecisionHistoryDigest.Build(entries);

        Assert.Single(result.Text.Split('\n'));
        Assert.DoesNotContain(new string('i', 201), result.Text);
        Assert.DoesNotContain(new string('r', 201), result.Text);
        Assert.DoesNotContain(new string('x', 301), result.Text);
    }

    /// <summary>Embedded newlines would break the one-line-per-decision contract.</summary>
    [Fact]
    public void Build_FlattensNewlinesInsideFields()
    {
        var entries = new List<DecisionHistoryEntry>
        {
            Entry(AgentDecisionStatus.Rejected, rejectionReason: "line one\nline two\nline three")
        };

        var result = DecisionHistoryDigest.Build(entries);

        Assert.Single(result.Text.Split('\n'));
        Assert.Contains("line one line two line three", result.Text);
    }

    [Fact]
    public void Build_RespectsTotalCharacterCap()
    {
        var entries = Enumerable.Range(0, 120)
            .Select(i => Entry(
                AgentDecisionStatus.Rejected,
                rejectionReason: new string('x', 300),
                toolInput: new string('i', 200),
                reasoning: new string('r', 200),
                minutesAgo: i))
            .ToList();

        var result = DecisionHistoryDigest.Build(entries);

        Assert.True(result.Text.Length <= 12_000, $"digest was {result.Text.Length} chars");
    }

    private static DecisionHistoryEntry Entry(
        AgentDecisionStatus status,
        string toolName = "assign_load_to_truck",
        string? toolInput = """{"load_id":"abc"}""",
        string? reasoning = "Closest truck",
        string? rejectionReason = null,
        int minutesAgo = 0)
    {
        return new DecisionHistoryEntry(
            status, toolName, toolInput, reasoning, rejectionReason, DateTime.UtcNow.AddMinutes(-minutesAgo));
    }
}
