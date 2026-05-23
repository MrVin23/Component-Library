using Microsoft.AspNetCore.Components;

namespace Client.Components.CustomSelect;

public partial class CustomDropdownPanel : ComponentBase
{
    /// <summary>
    /// Whether the dropdown panel is visible
    /// </summary>
    [Parameter]
    public bool IsOpen { get; set; }

    /// <summary>
    /// Callback when the panel should close
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// Child content to render inside the dropdown panel
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Additional CSS classes for the dropdown panel
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Reference to the dropdown panel element
    /// </summary>
    public ElementReference DropdownPanelRef { get; set; }
}
