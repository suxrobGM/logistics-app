namespace Logistics.BlazorComponents.Grids;

public class PageSettings
{
    public PageSettings()
    {
        OnPageChanged = (e) => Task.CompletedTask;
    }

    public int PagesCount { get; set; } = 1;
    public Func<PageEventArgs, Task> OnPageChanged { get; set; }
}
