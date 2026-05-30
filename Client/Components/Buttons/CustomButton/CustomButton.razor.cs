using Microsoft.AspNetCore.Components;

namespace Client.Components.Buttons.CustomButton;

public partial class CustomButton : ComponentBase
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Button text content
    /// </summary>
    [Parameter]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Child content to render inside the button
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Style type of the button component
    /// </summary>
    [Parameter]
    public CustomButtonType ButtonType { get; set; } = CustomButtonType.Primary;

    /// <summary>
    /// Visual size (padding and type scale).
    /// </summary>
    [Parameter]
    public CustomButtonSize Size { get; set; } = CustomButtonSize.Medium;

    /// <summary>
    /// Bootstrap icon class (e.g., "bi bi-check")
    /// </summary>
    [Parameter]
    public string IconClass { get; set; } = string.Empty;

    /// <summary>
    /// Position of the icon relative to text
    /// </summary>
    [Parameter]
    public ButtonIconPosition IconPosition { get; set; } = ButtonIconPosition.Left;

    /// <summary>
    /// Width of the button
    /// </summary>
    [Parameter]
    public string Width { get; set; } = "auto";

    /// <summary>
    /// Height of the button
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "auto";

    /// <summary>
    /// Optional overrides for <c>--btn-*</c> tokens (e.g. <c>--btn-fg: var(--app-primary);</c>).
    /// Used by ScrollMaster Button designer for live preview; omit everywhere else.
    /// </summary>
    [Parameter]
    public string? CssVariablesStyle { get; set; }

    /// <summary>
    /// Whether the button is disabled
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// HTML button type: "button", "submit", or "reset". Default "button" to prevent accidental form submission.
    /// </summary>
    [Parameter]
    public string ButtonHtmlType { get; set; } = "button";

    /// <summary>
    /// Callback when button is clicked
    /// </summary>
    [Parameter]
    public EventCallback<Microsoft.AspNetCore.Components.Web.MouseEventArgs> OnClick { get; set; }

    private async Task HandleClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
    {
        if (!Disabled)
        {
            await OnClick.InvokeAsync(args);
        }
    }

    private string ButtonCssClass
    {
        get
        {
            var typeClass = ButtonType switch
            {
                CustomButtonType.Primary => "custom-button primary",
                CustomButtonType.Secondary => "custom-button secondary",
                CustomButtonType.FormGreen => "custom-button form-outline form-green",
                CustomButtonType.FormGrey => "custom-button form-outline form-grey",
                CustomButtonType.FormBlue => "custom-button form-outline form-blue",
                CustomButtonType.FormPurple => "custom-button form-outline form-purple",
                CustomButtonType.Pager => "custom-button pager",
                CustomButtonType.Danger => "custom-button danger",
                CustomButtonType.Ghost => "custom-button ghost",
                CustomButtonType.Tertiary => "custom-button tertiary",
                CustomButtonType.Generate => "custom-button generate",
                _ => "custom-button primary"
            };

            var sizeClass = Size switch
            {
                CustomButtonSize.Small => " custom-button--sm",
                CustomButtonSize.Large => " custom-button--lg",
                _ => string.Empty
            };

            return typeClass + sizeClass;
        }
    }

    // CustomButton styles normally come from CSS classes (.primary, etc.). The designer passes
    // CssVariablesStyle so --btn-* values can be overridden on the element for real-time preview
    // without generating new stylesheet rules. Width and height stay separate parameters.
    private string InlineStyle
    {
        get
        {
            var parts = new List<string>
            {
                $"width:{Width};",
                $"height:{Height};"
            };

            if (!string.IsNullOrWhiteSpace(CssVariablesStyle))
                parts.Add(CssVariablesStyle.Trim());

            return string.Join(" ", parts);
        }
    }
}

/// <summary>Padding and font scale for <see cref="CustomButton"/>.</summary>
public enum CustomButtonSize
{
    Small,
    Medium,
    Large
}

public enum CustomButtonType
{
    /// <summary>
    /// Primary style - main action button
    /// </summary>
    Primary,

    /// <summary>
    /// Secondary style - alternative action button
    /// </summary>
    Secondary,

    /// <summary>
    /// Form green style - outline success button (matches Bootstrap btn-outline-success)
    /// </summary>
    FormGreen,

    /// <summary>
    /// Form grey style - outline secondary button (matches Bootstrap btn-outline-secondary)
    /// </summary>
    FormGrey,

    /// <summary>
    /// Form blue style - outline primary button (matches Bootstrap btn-outline-primary)
    /// </summary>
    FormBlue,

    /// <summary>
    /// Form purple style - outline purple button
    /// </summary>
    FormPurple,

    /// <summary>
    /// Pager style - compact button for pager components
    /// </summary>
    Pager,

    /// <summary>
    /// Danger style - destructive action button
    /// </summary>
    Danger,

    /// <summary>
    /// Ghost style - minimal/transparent button
    /// </summary>
    Ghost,

    /// <summary>
    /// Tertiary style — blue-cyan control; colors from app.css --app-tertiary-* (palette cyan).
    /// </summary>
    Tertiary,

    /// <summary>
    /// Generate style — glowy button with sparkles (distinct from tertiary).
    /// </summary>
    Generate
}

public enum ButtonIconPosition
{
    Left,
    Right
}

