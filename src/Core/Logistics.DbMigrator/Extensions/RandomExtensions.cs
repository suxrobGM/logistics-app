namespace Logistics.DbMigrator.Extensions;

public static class RandomExtensions
{
    public static T Pick<T>(this Random random, IList<T> list)
    {
        var rndIndex = random.Next(list.Count);
        return list[rndIndex];
    }
    
    public static DateTime Date(this Random random, DateTime minDate, DateTime maxDate)
    {
        var ticks = random.NextInt64(minDate.Ticks, maxDate.Ticks);
        return new DateTime(ticks);
    }
}
