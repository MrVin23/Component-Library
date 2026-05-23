using Microsoft.AspNetCore.Components;

namespace Client.Components.Table;

public partial class CustomTable<TItem> : ComponentBase
{
    #region Parameters

    /// <summary>
    /// Title displayed above the table
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Collection of items to display in the table
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// Optional max height for the table wrapper (e.g. "520px", "60vh").
    /// When set, the table area will scroll vertically if content exceeds this height.
    /// </summary>
    [Parameter]
    public string? MaxHeight { get; set; }

    /// <summary>
    /// Template for the table header row (th elements). Used when Columns is not provided.
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Column definitions for sortable tables (non-generic). When provided, overrides HeaderTemplate for header rendering.
    /// Requires RowTemplate to be provided for cell content.
    /// </summary>
    [Parameter]
    public List<TableColumn>? Columns { get; set; }

    /// <summary>
    /// Generic column definitions with value extraction. When provided, automatically renders both headers and cells.
    /// This is the recommended approach for simple tables.
    /// </summary>
    [Parameter]
    public List<TableColumn<TItem>>? ColumnDefinitions { get; set; }

    /// <summary>
    /// Template for each row (td elements). Required when using Columns, optional when using ColumnDefinitions.
    /// </summary>
    [Parameter]
    public RenderFragment<TItem>? RowTemplate { get; set; }

    /// <summary>
    /// Optional template for the footer row
    /// </summary>
    [Parameter]
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>
    /// Optional template for empty state
    /// </summary>
    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    /// <summary>
    /// Optional header actions (buttons, etc.)
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderActions { get; set; }

    /// <summary>
    /// Message to display when there are no items
    /// </summary>
    [Parameter]
    public string EmptyMessage { get; set; } = "No data available";

    /// <summary>
    /// Enable checkbox selection for rows
    /// </summary>
    [Parameter]
    public bool ShowCheckboxSelection { get; set; } = false;

    #endregion

    #region Loading/Refresh Parameters

    /// <summary>
    /// Indicates if the table is currently loading/refreshing data
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; } = false;

    /// <summary>
    /// Show a refresh button in the header
    /// </summary>
    [Parameter]
    public bool ShowRefreshButton { get; set; } = false;

    /// <summary>
    /// Callback when refresh button is clicked
    /// </summary>
    [Parameter]
    public EventCallback OnRefresh { get; set; }

    /// <summary>
    /// Message to display while loading
    /// </summary>
    [Parameter]
    public string LoadingMessage { get; set; } = "Loading...";

    #endregion

    #region Selection Parameters

    /// <summary>
    /// Currently selected items (two-way bindable)
    /// </summary>
    [Parameter]
    public HashSet<TItem> SelectedItems { get; set; } = new();

    /// <summary>
    /// Callback when selected items change
    /// </summary>
    [Parameter]
    public EventCallback<HashSet<TItem>> SelectedItemsChanged { get; set; }

    /// <summary>
    /// Function to get unique key for each item (required for selection)
    /// </summary>
    [Parameter]
    public Func<TItem, object>? ItemKey { get; set; }

    /// <summary>
    /// Apply striped row styling
    /// </summary>
    [Parameter]
    public bool Striped { get; set; } = true;

    /// <summary>
    /// Apply hover effect on rows
    /// </summary>
    [Parameter]
    public bool Hoverable { get; set; } = true;

    /// <summary>
    /// Apply borders to cells
    /// </summary>
    [Parameter]
    public bool Bordered { get; set; } = false;

    /// <summary>
    /// Function to determine custom CSS class for a row
    /// </summary>
    [Parameter]
    public Func<TItem, string>? RowClass { get; set; }

    /// <summary>
    /// Callback when a row is clicked
    /// </summary>
    [Parameter]
    public EventCallback<TItem> OnRowClick { get; set; }

    /// <summary>
    /// Number of columns (for colspan in empty state)
    /// </summary>
    [Parameter]
    public int ColumnCount { get; set; } = 1;

    #endregion

    #region Sorting Parameters

    /// <summary>
    /// Enable sorting on the table
    /// </summary>
    [Parameter]
    public bool Sortable { get; set; } = false;

    /// <summary>
    /// Current sort column name
    /// </summary>
    [Parameter]
    public string? SortColumn { get; set; }

    /// <summary>
    /// Callback when sort column changes
    /// </summary>
    [Parameter]
    public EventCallback<string?> SortColumnChanged { get; set; }

    /// <summary>
    /// Current sort direction (true = ascending, false = descending)
    /// </summary>
    [Parameter]
    public bool SortAscending { get; set; } = true;

    /// <summary>
    /// Callback when sort direction changes
    /// </summary>
    [Parameter]
    public EventCallback<bool> SortAscendingChanged { get; set; }

    /// <summary>
    /// Callback when sort is requested (provides column name and direction)
    /// </summary>
    [Parameter]
    public EventCallback<(string Column, bool Ascending)> OnSort { get; set; }

    #endregion

    #region Pagination Parameters

    /// <summary>
    /// Show pagination controls
    /// </summary>
    [Parameter]
    public bool ShowPagination { get; set; } = false;

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    [Parameter]
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Callback when page changes
    /// </summary>
    [Parameter]
    public EventCallback<int> CurrentPageChanged { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    [Parameter]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Total number of items (for pagination calculation)
    /// </summary>
    [Parameter]
    public int TotalItems { get; set; } = 0;

    #endregion

    #region Computed Properties

    private int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 1;

    private bool IsAllSelected => Items != null && Items.Any() && Items.All(IsItemSelected);

    #endregion

    #region Methods

    private int GetColumnCount()
    {
        var count = ColumnDefinitions?.Count ?? Columns?.Count ?? ColumnCount;
        if (ShowCheckboxSelection) count++;
        return Math.Max(count, 1);
    }

    private bool IsItemSelected(TItem item)
    {
        if (ItemKey != null)
        {
            var key = ItemKey(item);
            return SelectedItems.Any(i => ItemKey(i)?.Equals(key) == true);
        }
        return SelectedItems.Contains(item);
    }

    private async Task OnItemSelectionChanged(TItem item, bool isSelected)
    {
        if (isSelected)
        {
            SelectedItems.Add(item);
        }
        else
        {
            if (ItemKey != null)
            {
                var key = ItemKey(item);
                var itemToRemove = SelectedItems.FirstOrDefault(i => ItemKey(i)?.Equals(key) == true);
                if (itemToRemove != null)
                {
                    SelectedItems.Remove(itemToRemove);
                }
            }
            else
            {
                SelectedItems.Remove(item);
            }
        }
        await SelectedItemsChanged.InvokeAsync(SelectedItems);
    }

    private async Task OnSelectAllChanged(ChangeEventArgs e)
    {
        var isSelected = (bool)e.Value!;
        
        if (isSelected && Items != null)
        {
            foreach (var item in Items)
            {
                if (!IsItemSelected(item))
                {
                    SelectedItems.Add(item);
                }
            }
        }
        else
        {
            if (Items != null && ItemKey != null)
            {
                foreach (var item in Items)
                {
                    var key = ItemKey(item);
                    var itemToRemove = SelectedItems.FirstOrDefault(i => ItemKey(i)?.Equals(key) == true);
                    if (itemToRemove != null)
                    {
                        SelectedItems.Remove(itemToRemove);
                    }
                }
            }
            else if (Items != null)
            {
                foreach (var item in Items)
                {
                    SelectedItems.Remove(item);
                }
            }
        }
        
        await SelectedItemsChanged.InvokeAsync(SelectedItems);
    }

    private async Task OnRowClicked(TItem item)
    {
        await OnRowClick.InvokeAsync(item);
    }

    private async Task OnRefreshClicked()
    {
        if (!IsLoading)
        {
            await OnRefresh.InvokeAsync();
        }
    }

    private async Task OnPageChanged(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            await CurrentPageChanged.InvokeAsync(page);
        }
    }

    private async Task OnSortClicked(string columnName)
    {
        if (!Sortable) return;

        bool newAscending;
        if (SortColumn == columnName)
        {
            // Toggle direction if same column
            newAscending = !SortAscending;
        }
        else
        {
            // New column, default to ascending
            newAscending = true;
        }

        SortColumn = columnName;
        SortAscending = newAscending;

        await SortColumnChanged.InvokeAsync(SortColumn);
        await SortAscendingChanged.InvokeAsync(SortAscending);
        await OnSort.InvokeAsync((columnName, newAscending));
    }

    private string GetSortIndicator(string columnName)
    {
        if (SortColumn != columnName) return "";
        return SortAscending ? "▲" : "▼";
    }

    #endregion
}
