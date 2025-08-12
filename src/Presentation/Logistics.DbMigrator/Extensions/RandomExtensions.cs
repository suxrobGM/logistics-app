namespace Logistics.DbMigrator.Extensions;

public static class RandomExtensions
{
    public static T Pick<T>(this Random random, IList<T> list)
    {
        var rndIndex = random.Next(list.Count);
        return list[rndIndex];
    }

    public static DateTime UtcDate(this Random random, DateTime from, DateTime to)
    {
        var spanSec = (long)(to - from).TotalSeconds;
        var date = from.AddSeconds(random.NextInt64(spanSec));
        return DateTime.SpecifyKind(date, DateTimeKind.Utc);
    }
}
