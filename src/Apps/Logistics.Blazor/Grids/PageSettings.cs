namespace Logistics.Blazor.Grids;

public class PageSettings
{
    public PageSettings()
    {
        OnPageChanged = (e) => Task.CompletedTask;
    }

    public int PagesCount { get; set; } = 2;
    public Func<PageEventArgs, Task> OnPageChanged { get; set; }
}
