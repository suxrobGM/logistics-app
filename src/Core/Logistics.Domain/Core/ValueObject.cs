namespace Logistics.Domain.Core;

public abstract record ValueObject
{
    protected static bool EqualOperator(ValueObject? left, ValueObject? right)
    {
        if (left is null ^ right is null)
        {
            return false;
        }
        return left is null || left.Equals(right!);
    }

    protected static bool NotEqualOperator(ValueObject? left, ValueObject? right)
    {
        return !EqualOperator(left, right);
    }

    protected abstract IEnumerable<object?> GetEqualityComponents();
    
    public virtual bool Equals(ValueObject? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        return GetEqualityComponents().SequenceEqual(obj.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }
}
