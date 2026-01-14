using Logistics.DbMigrator.Abstractions;

namespace Logistics.DbMigrator.Services;

/// <summary>
/// Validates and orders seeders using topological sort.
/// </summary>
internal static class SeederRegistry
{
    /// <summary>
    /// Returns seeders in execution order:
    /// 1. Infrastructure seeders first (by Order, then by dependency)
    /// 2. FakeData seeders second (by Order, then by dependency)
    /// </summary>
    public static IReadOnlyList<ISeeder> GetOrderedSeeders(IEnumerable<ISeeder> seeders, ILogger logger)
    {
        var seederList = seeders.ToList();
        var seederMap = seederList.ToDictionary(s => s.Name);
        ValidateDependencies(seederList, seederMap);

        var infrastructure = seederList.Where(s => s.Type == SeederType.Infrastructure);
        var fakeData = seederList.Where(s => s.Type == SeederType.FakeData);

        var orderedInfra = TopologicalSort(infrastructure, seederMap);
        var orderedFake = TopologicalSort(fakeData, seederMap);

        logger.LogDebug("Seeder execution order: {Order}",
            string.Join(" -> ", orderedInfra.Concat(orderedFake).Select(s => s.Name)));

        return [.. orderedInfra, .. orderedFake];
    }

    private static void ValidateDependencies(List<ISeeder> seeders, Dictionary<string, ISeeder> seederMap)
    {
        foreach (var seeder in seeders)
        {
            foreach (var dep in seeder.DependsOn)
            {
                if (!seederMap.ContainsKey(dep))
                {
                    throw new InvalidOperationException(
                        $"Seeder '{seeder.Name}' depends on '{dep}' which does not exist");
                }
            }
        }
    }

    private static List<ISeeder> TopologicalSort(
        IEnumerable<ISeeder> seeders,
        Dictionary<string, ISeeder> allSeeders)
    {
        var result = new List<ISeeder>();
        var inDegree = new Dictionary<string, int>();
        var adjacency = new Dictionary<string, List<string>>();

        var seederList = seeders.ToList();

        foreach (var s in seederList)
        {
            inDegree[s.Name] = 0;
            adjacency[s.Name] = [];
        }

        foreach (var s in seederList)
        {
            foreach (var dep in s.DependsOn.Where(d => inDegree.ContainsKey(d)))
            {
                adjacency[dep].Add(s.Name);
                inDegree[s.Name]++;
            }
        }

        // Priority queue ordered by Order property
        var queue = new PriorityQueue<ISeeder, int>();
        foreach (var s in seederList.Where(s => inDegree[s.Name] == 0))
        {
            queue.Enqueue(s, s.Order);
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            foreach (var dependent in adjacency[current.Name])
            {
                inDegree[dependent]--;
                if (inDegree[dependent] == 0)
                {
                    var depSeeder = allSeeders[dependent];
                    queue.Enqueue(depSeeder, depSeeder.Order);
                }
            }
        }

        if (result.Count != seederList.Count)
        {
            throw new InvalidOperationException("Circular dependency detected in seeders");
        }

        return result;
    }
}
