namespace Logistics.Infrastructure.AI.Tools;

/// <summary>Which of <c>AgentDecision</c>'s entity links a tool input fills.</summary>
internal enum AgentEntityKind
{
    Load,
    Truck,
    Trip,
    Invoice,
    Customer,
    Negotiation
}
