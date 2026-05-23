using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Client.Components.Buttons.SwitchButton;

public partial class CustomSwitchButton
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    /// <summary>Visual/behavior mode. Default is <see cref="SwitchButtonVariant.Normal"/>.</summary>
    [Parameter]
    public SwitchButtonVariant Variant { get; set; } = SwitchButtonVariant.Normal;

    [Parameter]
    public bool Value { get; set; }

    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    /// <summary>When <see cref="Variant"/> is <see cref="SwitchButtonVariant.DarkLightMode"/>, set false if a parent (e.g. <c>ThemeHandler</c>) applies the theme.</summary>
    [Parameter]
    public bool ApplyBuiltinTheme { get; set; } = true;

    [Parameter]
    public bool Disabled { get; set; }

    private async Task ToggleAsync()
    {
        if (Disabled)
            return;

        var next = !Value;
        await ValueChanged.InvokeAsync(next);

        if (Variant == SwitchButtonVariant.DarkLightMode && ApplyBuiltinTheme)
            await ApplyThemeAsync(next);
    }

    private async Task ApplyThemeAsync(bool isDark)
    {
        try
        {
            await JS.InvokeVoidAsync("themeInterop.setTheme", isDark ? "dark" : "light");
        }
        catch (JSException)
        {
            // Ignore if script is not available yet
        }
    }
}
