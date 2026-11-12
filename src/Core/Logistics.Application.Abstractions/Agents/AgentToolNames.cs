namespace Logistics.Application.Abstractions.Agents;

/// <summary>Tool names that prompts and handlers refer to outside the tool itself.</summary>
public static class AgentToolNames
{
    public const string GetTrip = "get_trip";
    public const string GetDriverHosStatus = "get_driver_hos_status";
    public const string SendCustomerUpdate = "send_customer_update";
    public const string GetNegotiationThread = "get_negotiation_thread";
    public const string ProposeCounterOffer = "propose_counter_offer";
}
