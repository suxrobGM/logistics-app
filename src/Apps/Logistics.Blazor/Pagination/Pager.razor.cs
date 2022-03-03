namespace Logistics.Blazor.Pagination;

public partial class Pager : ComponentBase
{
    [Parameter]
    public int CurrentPage { get; set; } = 1;

    [Parameter]
    public int PagesCount { get; set; } = 2;

    [Parameter]
    public Func<int, Task>? OnPageChanged { get; set; }

    private void PagerButtonClicked(int page)
    {
        CurrentPage = page;
        OnPageChanged?.Invoke(page);
    }
}
