namespace Logistics.Infrastructure.AI.Tools;

/// <summary>
/// Marks an input property that carries an entity id, so the decision audit row can link to it.
/// The wire key comes from the property, the same way the schema does, so renaming the property
/// moves the audit link with it instead of silently dropping it.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class AgentEntityIdAttribute(AgentEntityKind kind) : Attribute
{
    public AgentEntityKind Kind { get; } = kind;
}
