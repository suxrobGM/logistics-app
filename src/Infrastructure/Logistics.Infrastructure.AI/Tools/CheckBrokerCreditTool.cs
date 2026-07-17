using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Infrastructure.AI.Tools;

internal sealed class CheckBrokerCreditTool(IBrokerCreditService brokerCreditService) : IAiDispatchTool
{
    public string Name => "check_broker_credit";

    public async Task<string> ExecuteAsync(JsonNode input, CancellationToken ct)
    {
        var mcNumber = input["mc_number"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(mcNumber))
        {
            return JsonSerializer.Serialize(new { error = "Missing mc_number" });
        }

        var credit = await brokerCreditService.GetBrokerCreditAsync(mcNumber, ct);
        if (credit is null)
        {
            return JsonSerializer.Serialize(new
            {
                mc_number = mcNumber,
                credit_score = (int?)null,
                warning = "No credit data available for this broker. Booking is allowed but flag the missing data to the dispatcher."
            });
        }

        return JsonSerializer.Serialize(new
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
