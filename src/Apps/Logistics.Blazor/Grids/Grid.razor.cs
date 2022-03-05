using System.ComponentModel;

namespace Logistics.Blazor.Grids;

public partial class Grid<TData> : ComponentBase
{
#pragma warning disable CS8618
    private Spinner spinnerRef;
#pragma warning restore CS8618

    private readonly List<Column> columns = new();

    [Parameter]
    public IEnumerable<TData>? DataSource { get; set; }

    [Parameter]
    public RenderFragment? Columns { get; set; }

    [Parameter]
    public bool UseBorderlessCells { get; set; } = true;

    [Parameter]
    public bool AllowSorting { get; set; }

    [Parameter]
    public bool AllowPaging { get; set; }

    [Parameter]
    public PageSettings PageSettings { get; set; } = new();

    internal void AddColumn(Column column)
    {
        columns.Add(column);
    }

    private async Task SortAsync(Column? column)
    {
        if (column == null || string.IsNullOrEmpty(column.Field))
        {
            return;
        }
        
        if (column.SortDirection == null)
        {
            column.SortDirection = SortDirection.Ascending;
        }
        else if (column.SortDirection == SortDirection.Ascending)
        {
            column.SortDirection = SortDirection.Descending;
        }
        else if (column.SortDirection == SortDirection.Descending)
        {
            column.SortDirection = SortDirection.Ascending;
        }

        await SortAsync(column.Field, column.SortDirection.Value);
    }

    public async Task SortAsync(string? columnName, SortDirection direction)
    {
        if (DataSource == null || string.IsNullOrEmpty(columnName))
            return;

        var prop = TypeDescriptor.GetProperties(typeof(TData)).Find(columnName, false);

        if (prop == null)
            return;

        await spinnerRef.ShowAsync();
        if (direction == SortDirection.Ascending)
        {
            DataSource = DataSource.OrderBy(i => prop.GetValue(i));
        }
        else
        {
            DataSource = DataSource.OrderByDescending(i => prop.GetValue(i));
        }

        ResetSortFlags(columnName);
        await spinnerRef.HideAsync();
    }

    private void ResetSortFlags(string? excludeColumnName)
    {
        if (string.IsNullOrEmpty(excludeColumnName))
            return;

        foreach (var column in columns)
        {
            if (column.Field == excludeColumnName)
                continue;

            column.SortDirection = null;
        }
    }

    private async Task PageChangedHandler(PageEventArgs e)
    {
        await spinnerRef.ShowAsync();
        await PageSettings.OnPageChanged.InvokeAsync(e);
        await spinnerRef.HideAsync();
    }

    private string GetTextAlign(TextAlign textAlign)
    {
        return textAlign switch
        {
            TextAlign.Left => "left",
            TextAlign.Right => "right",
            TextAlign.Center => "center",
            TextAlign.Justify => "justify",
            _ => "left",
        };
    }
}
