namespace Logistics.Mediator.Tests.TestKit;

/// <summary>Ordered record of what ran, shared by the fakes in one test.</summary>
public sealed class Trace
{
    private readonly List<string> _entries = [];

    public IReadOnlyList<string> Entries => _entries;

    public void Add(string entry) => _entries.Add(entry);
}
