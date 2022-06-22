using System.Reflection;

namespace Logistics.Domain;

public abstract class Enumeration : IComparable
{
    protected Enumeration(int id, string name) => (Id, Name) = (id, name);
    
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    public string Name { get; private set; }
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
    // ReSharper disable once MemberCanBePrivate.Global
    public int Id { get; private set; }

    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public int CompareTo(object? other) => Id.CompareTo((other as Enumeration)?.Id);

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        return typeof(T)
                .GetFields(BindingFlags.Public |
                           BindingFlags.Static |
                           BindingFlags.DeclaredOnly)
                .Select(f => f.GetValue(null))
                .Cast<T>();
    }

    public static bool operator ==(Enumeration left, Enumeration right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Enumeration left, Enumeration right)
    {
        return !(left == right);
    }

    public static bool operator <(Enumeration left, Enumeration right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(Enumeration left, Enumeration right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(Enumeration left, Enumeration right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(Enumeration left, Enumeration right)
    {
        return left.CompareTo(right) >= 0;
    }
}
