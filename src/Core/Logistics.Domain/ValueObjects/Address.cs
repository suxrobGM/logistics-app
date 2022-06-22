using Logistics.Domain.SeedWork;

namespace Logistics.Domain.ValueObjects;

public class Address : ValueObject
{
    public string? Street { get; init; }
    public string? City { get; init; }
    public string? ZipCode { get; init; }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street!;
        yield return City!;
        yield return ZipCode!;
    }
}