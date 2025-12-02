namespace Logistics.DbMigrator.Extensions;

public static class RandomExtensions
{
    extension(Random random)
    {
        public T Pick<T>(IList<T> list)
        {
            var rndIndex = random.Next(list.Count);
            return list[rndIndex];
        }

        public DateTime UtcDate(DateTime from, DateTime to)
        {
            var spanSec = (long)(to - from).TotalSeconds;
            var date = from.AddSeconds(random.NextInt64(spanSec));
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
    }
}
