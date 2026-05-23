using Microsoft.AspNetCore.Components;

namespace Client.Components.CustomSelect;

public partial class CustomSelect<TItem> : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Height of the select element
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "auto";

    /// <summary>
    /// Width of the select element
    /// </summary>
    [Parameter]
    public string Width { get; set; } = "100%";

    /// <summary>
    /// Label text displayed above the select
    /// </summary>
    [Parameter]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Style type of the select component
    /// </summary>
    [Parameter]
    public CustomSelectType SelectType { get; set; } = CustomSelectType.Primary;

    /// <summary>
    /// List of items to display in the select
    /// </summary>
    [Parameter]
    public List<TItem> Items { get; set; } = new();

    /// <summary>
    /// Function to get the display text for an item
    /// </summary>
    [Parameter]
    public Func<TItem, string> DisplayFunc { get; set; } = item => item?.ToString() ?? string.Empty;

    /// <summary>
    /// Function to get the value for an item
    /// </summary>
    [Parameter]
    public Func<TItem, string> ValueFunc { get; set; } = item => item?.ToString() ?? string.Empty;

    /// <summary>
    /// Currently selected value
    /// </summary>
    [Parameter]
    public TItem? SelectedValue { get; set; }

    /// <summary>
    /// Callback when selected value changes
    /// </summary>
    [Parameter]
    public EventCallback<TItem?> SelectedValueChanged { get; set; }

    /// <summary>
    /// Placeholder text when no item is selected
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Select...";

    /// <summary>
    /// Whether the select is disabled
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; } = false;

    private string SelectedValueString
    {
        get => SelectedValue != null ? ValueFunc(SelectedValue) : string.Empty;
        set
        {
            var selectedItem = Items.FirstOrDefault(item => ValueFunc(item) == value);
            if (!EqualityComparer<TItem>.Default.Equals(selectedItem, SelectedValue))
            {
                SelectedValue = selectedItem;
                SelectedValueChanged.InvokeAsync(selectedItem);
            }
        }
    }

    private string ContainerCssClass => SelectType switch
    {
        CustomSelectType.Pager => "custom-select-container pager",
        _ => "custom-select-container primary"
    };

    private string SelectCssClass => SelectType switch
    {
        CustomSelectType.Pager => "custom-select pager",
        _ => "custom-select primary"
    };

    private string LabelCssClass => SelectType switch
    {
        CustomSelectType.Pager => "custom-select-label pager",
        _ => "custom-select-label primary"
    };
}

public enum CustomSelectType
{
    /// <summary>
    /// Primary style - standard form select
    /// </summary>
    Primary,
    
    /// <summary>
    /// Pager style - compact select for pager components
    /// </summary>
    Pager
}