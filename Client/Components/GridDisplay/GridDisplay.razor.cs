using Microsoft.AspNetCore.Components;

namespace Client.Components.GridDisplay;

public partial class GridDisplay : ComponentBase
{
    [Parameter]
    public IReadOnlyList<string>? ColumnHeaders { get; set; }

    /// <summary>Data columns when <see cref="ColumnHeaders"/> is omitted (required if rows have no column header row).</summary>
    [Parameter]
    public int? ColumnCount { get; set; }

    /// <summary>When true, the grid reserves a leading column for <see cref="GridDisplayRow"/> labels.</summary>
    [Parameter]
    public bool RowHeaders { get; set; } = true;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Border { get; set; } = true;

    [Parameter]
    public bool Padding { get; set; } = true;

    private int _rowIndex;

    internal int RegisterRow() => _rowIndex++;

    protected bool HasColumnHeaders => ColumnHeaders is { Count: > 0 };

    protected bool ShowCorner => HasColumnHeaders && RowHeaders;

    protected int DataColumnCount => ColumnHeaders?.Count ?? ColumnCount ?? 1;

    protected string MatrixCssClass =>
        string.Join(
            " ",
            new[]
            {
                "grid-display__matrix",
                HasColumnHeaders ? "grid-display__matrix--with-col-headers" : null,
                RowHeaders ? "grid-display__matrix--with-row-headers" : null,
                ShowCorner ? "grid-display__matrix--with-corner" : null,
            }.Where(static s => !string.IsNullOrWhiteSpace(s)));

    protected string MatrixStyle
    {
        get
        {
            var columns = new List<string>();
            if (RowHeaders)
                columns.Add("minmax(calc(var(--spacing-base) * 30), auto)");
            if (DataColumnCount > 0)
                columns.Add(string.Join(" ", Enumerable.Repeat("minmax(0, 1fr)", DataColumnCount)));
            return columns.Count == 0
                ? string.Empty
                : $"grid-template-columns: {string.Join(" ", columns)};";
        }
    }
}
