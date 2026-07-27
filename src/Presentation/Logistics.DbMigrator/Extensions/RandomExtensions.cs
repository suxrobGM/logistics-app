namespace Logistics.DbMigrator.Extensions;

public static class RandomExtensions
{
    extension(Random random)
    {
        public T Pick<T>(IList<T> list)
        {
            // Next(0) returns 0, so without this the indexer throws an opaque out-of-range error.
            if (list.Count == 0)
            {
                throw new ArgumentException($"Cannot pick from an empty {typeof(T).Name} list", nameof(list));
            }

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
