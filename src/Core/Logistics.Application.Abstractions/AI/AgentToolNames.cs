namespace Logistics.Application.Abstractions.AI;

/// <summary>Names of the agent tools that prompts and handlers mention by name.</summary>
public static class AgentToolNames
{
    public const string GetTrip = "get_trip";
    public const string GetDriverHosStatus = "get_driver_hos_status";
    public const string SendCustomerUpdate = "send_customer_update";
    public const string GetNegotiationThread = "get_negotiation_thread";
    public const string ProposeCounterOffer = "propose_counter_offer";
}
