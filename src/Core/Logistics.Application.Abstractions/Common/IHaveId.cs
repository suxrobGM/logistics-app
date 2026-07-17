namespace Logistics.Application.Abstractions.Common;

/// <summary>Request that targets a single entity by primary key.</summary>
public interface IHaveId
{
    Guid Id { get; }
}
