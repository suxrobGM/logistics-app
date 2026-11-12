namespace Logistics.Application.Abstractions.Agents;

/// <summary>
///     Tool names that code outside the tool itself refers to - prompts that tell the agent which tool to call,
///     and handlers that recognise a tool's decision. Renaming a tool here renames it everywhere.
/// </summary>
public static class AgentToolNames
{
    public const string GetTrip = "get_trip";
    public const string GetDriverHosStatus = "get_driver_hos_status";
    public const string SendCustomerUpdate = "send_customer_update";
    public const string GetNegotiationThread = "get_negotiation_thread";
    public const string ProposeCounterOffer = "propose_counter_offer";
}
