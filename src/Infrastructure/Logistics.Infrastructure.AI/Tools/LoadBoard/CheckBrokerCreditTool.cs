using System.ComponentModel;
using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Policies;

namespace Logistics.Infrastructure.AI.Tools.LoadBoard;

internal sealed class CheckBrokerCreditTool(IBrokerCreditService brokerCreditService)
    : AgentTool<CheckBrokerCreditTool.Input>, IAgentToolMetadata
{
    internal sealed record Input
    {
        [Description("The broker's MC number, e.g. 'MC123456' or '123456'")]
        public required string McNumber { get; init; }
    }

    public static AgentToolDefinition Definition => new(
        "check_broker_credit",
        "Check a load-board broker's credit standing (score 0-100, days-to-pay, FMCSA authority status) by MC number. Always call this before book_loadboard_load; never book when the score is below the tenant minimum or the authority is inactive.")
    {
        RequiredFeature = TenantFeature.LoadBoard,
        RequiredPermission = Permission.Dispatch.View,
        DispatchAgent = true
    };

    protected override async Task<string> ExecuteAsync(Input input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.McNumber))
            return ToolResult.Error("mc_number is blank - send the broker's MC number.");

        var credit = await brokerCreditService.GetBrokerCreditAsync(input.McNumber, ct);
        if (credit is null)
        {
            return ToolResult.Ok(new
            {
                mc_number = input.McNumber,
                credit_score = (int?)null,
                warning = "No credit data available for this broker. Booking is allowed but flag the missing data to the dispatcher."
            });
        }

        return ToolResult.Ok(new
        {
            mc_number = credit.McNumber,
            credit_score = credit.CreditScore,
            days_to_pay = credit.DaysToPay,
            authority_active = credit.AuthorityActive,
            source = credit.Source.ToString(),
            checked_at = credit.CheckedAt
        });
    }
}
