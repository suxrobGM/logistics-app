namespace Logistics.DbMigrator.Extensions;

public static class RandomExtensions
{
    extension(Random random)
    {
        public T Pick<T>(IList<T> list)
        {
            // Without this, an empty list yields random.Next(0) == 0 and an opaque
            // ArgumentOutOfRangeException from the indexer, which fails the whole migrator run.
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
