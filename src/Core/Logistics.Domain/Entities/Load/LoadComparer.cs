namespace Logistics.Domain.Entities;

internal class LoadComparer : IEqualityComparer<Load>
{
    public bool Equals(Load? x, Load? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null))
        {
            return false;
        }

        if (ReferenceEquals(y, null))
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Number == y.Number && x.Name == y.Name;
    }

    public int GetHashCode(Load? obj)
    {
        return HashCode.Combine(obj?.Number, obj?.Name);
    }
}
