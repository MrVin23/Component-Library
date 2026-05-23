using Microsoft.AspNetCore.Components;

namespace Client.Components.Table;

/// <summary>
/// Defines a column for the CustomTable component (non-generic version for backwards compatibility)
/// </summary>
public class TableColumn
{
    /// <summary>
    /// Display title for the column header
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Key used for sorting (if sortable). If null, uses Title.
    /// </summary>
    public string? SortKey { get; set; }

    /// <summary>
    /// Whether this column is sortable
    /// </summary>
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// CSS class to apply to the header cell
    /// </summary>
    public string? HeaderClass { get; set; }

    /// <summary>
    /// CSS class to apply to body cells in this column
    /// </summary>
    public string? CellClass { get; set; }

    /// <summary>
    /// Gets the effective sort key (SortKey or Title)
    /// </summary>
    public string EffectiveSortKey => SortKey ?? Title;
}

/// <summary>
/// Generic column definition with value extraction and template support
/// </summary>
/// <typeparam name="TItem">The type of item in the table</typeparam>
public class TableColumn<TItem> : TableColumn
{
    /// <summary>
    /// Function to extract the display value from an item
    /// </summary>
    public Func<TItem, object?>? Value { get; set; }

    /// <summary>
    /// Format string for the value (e.g., "dd MMM yyyy" for dates, "C2" for currency)
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Custom render template for the cell. If provided, overrides Value and Format.
    /// </summary>
    public RenderFragment<TItem>? Template { get; set; }

    /// <summary>
    /// Gets the formatted value for display
    /// </summary>
    public string GetFormattedValue(TItem item)
    {
        if (Value == null) return string.Empty;
        
        var value = Value(item);
        if (value == null) return string.Empty;

        if (!string.IsNullOrEmpty(Format) && value is IFormattable formattable)
        {
            return formattable.ToString(Format, null);
        }

        return value.ToString() ?? string.Empty;
    }
}
