using System.ComponentModel;

namespace Logistics.Blazor.Grids;

public partial class Grid<TData> : ComponentBase
{
    private readonly List<Column> columns = new();

    [Parameter]
    public IEnumerable<TData>? DataSource { get; set; }

    [Parameter]
    public RenderFragment? Columns { get; set; }

    [Parameter]
    public bool UseBorderlessCells { get; set; } = true;

    [Parameter]
    public bool AllowSorting { get; set; }

    internal void AddColumn(Column column)
    {
        columns.Add(column);
    }

    private void Sort(Column? column)
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

        Sort(column.Field, column.SortDirection.Value);
    }

    public void Sort(string? columnName, SortDirection direction)
    {
        if (DataSource == null || string.IsNullOrEmpty(columnName))
            return;

        var prop = TypeDescriptor.GetProperties(typeof(TData)).Find(columnName, false);

        if (prop == null)
            return;

        if (direction == SortDirection.Ascending)
        {
            DataSource = DataSource.OrderBy(i => prop.GetValue(i));
        }
        else
        {
            DataSource = DataSource.OrderByDescending(i => prop.GetValue(i));
        }

        ResetSortFlags(columnName);
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
