using Microsoft.AspNetCore.Components;

namespace Client.Components.Table;

/// <summary>
/// Defines an additional action option for the TableActionsCell dropdown menu.
/// </summary>
/// <typeparam name="TItem">The type of item in the table row</typeparam>
public class TableActionOption<TItem>
{
    /// <summary>
    /// Display label for the action
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Bootstrap icon class (e.g. "bi bi-eye", "bi bi-download")
    /// </summary>
    public string Icon { get; set; } = "bi bi-circle";

    /// <summary>
    /// Optional CSS class for styling (e.g. "table-actions-dropdown-danger" for delete)
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Callback when the action is clicked
    /// </summary>
    public EventCallback<TItem> OnClick { get; set; }

    /// <summary>
    /// Whether this action is disabled
    /// </summary>
    public bool IsDisabled { get; set; }
}
