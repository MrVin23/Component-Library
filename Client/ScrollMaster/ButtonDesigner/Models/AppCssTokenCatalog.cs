namespace Client.ScrollMaster.ButtonDesigner.Models;

/// <summary>
/// Dropdown values sourced from global design tokens in Client/wwwroot/css (non-scroll).
/// </summary>
public static class AppCssTokenCatalog
{
    public sealed record TokenOption(string Label, string Value);

    private static readonly TokenOption[] ColorTokens =
    [
        new("Text primary", "var(--text-color-primary)"),
        new("Text secondary", "var(--text-color-secondary)"),
        new("Text muted", "var(--text-color-muted)"),
        new("Text inverse", "var(--text-color-inverse)"),
        new("Theme background", "var(--theme-bg)"),
        new("Theme foreground", "var(--theme-fg)"),
        new("Theme foreground muted", "var(--theme-fg-muted)"),
        new("Theme foreground contrast", "var(--theme-fg-contrast)"),
        new("Theme border", "var(--theme-border)"),
        new("Theme surface muted", "var(--theme-surface-muted)"),
        new("App primary", "var(--app-primary)"),
        new("App primary soft background", "var(--app-primary-soft-bg)"),
        new("App primary soft border", "var(--app-primary-soft-border)"),
        new("App primary hover background", "var(--app-primary-hover-bg)"),
        new("App primary active background", "var(--app-primary-active-bg)"),
        new("App primary focus ring", "var(--app-primary-focus-ring)"),
        new("App orange foreground", "var(--app-orange-fg)"),
        new("App orange soft background", "var(--app-orange-soft-bg)"),
        new("App orange soft border", "var(--app-orange-soft-border)"),
        new("App orange hover background", "var(--app-orange-hover-bg)"),
        new("App orange hover foreground", "var(--app-orange-hover-fg)"),
        new("App orange active background", "var(--app-orange-active-bg)"),
        new("App orange active foreground", "var(--app-orange-active-fg)"),
        new("App orange focus ring", "var(--app-orange-focus-ring)"),
        new("App tertiary foreground", "var(--app-tertiary-fg)"),
        new("App tertiary soft background", "var(--app-tertiary-soft-bg)"),
        new("App tertiary soft border", "var(--app-tertiary-soft-border)"),
        new("App tertiary hover background", "var(--app-tertiary-hover-bg)"),
        new("App tertiary hover foreground", "var(--app-tertiary-hover-fg)"),
        new("App tertiary active background", "var(--app-tertiary-active-bg)"),
        new("App tertiary active foreground", "var(--app-tertiary-active-fg)"),
        new("App tertiary focus ring", "var(--app-tertiary-focus-ring)"),
        new("Palette purple", "var(--palette-purple)"),
        new("Palette orange", "var(--palette-orange)"),
        new("Palette yellow", "var(--palette-yellow)"),
        new("Palette cyan", "var(--palette-cyan)"),
        new("Palette accent 1", "var(--palette-accent-1)"),
        new("Palette accent 2", "var(--palette-accent-2)"),
        new("Palette accent 3", "var(--palette-accent-3)"),
        new("Palette accent 4", "var(--palette-accent-4)"),
        new("Support blue", "var(--support-blue)"),
        new("Support green", "var(--support-green)"),
        new("Support red", "var(--support-red)"),
        new("Support yellow", "var(--support-yellow)"),
        new("Support purple", "var(--support-purple)"),
        new("Support orange", "var(--support-orange)"),
    ];

    private static readonly TokenOption[] ShadowTokens =
    [
        new("Shadow XS", "var(--shadow-xs)"),
        new("Shadow SM", "var(--shadow-sm)"),
        new("Shadow MD", "var(--shadow-md)"),
        new("Shadow LG", "var(--shadow-lg)"),
        new("App shadow SM", "var(--app-shadow-sm)"),
        new("App shadow MD", "var(--app-shadow-md)"),
        new("App shadow LG", "var(--app-shadow-lg)"),
        new("Primary lift shadow", "var(--app-primary-shadow-lift)"),
        new("Orange lift shadow", "var(--app-orange-shadow-lift)"),
        new("Tertiary lift shadow", "var(--app-tertiary-shadow-lift)"),
        new("Primary focus ring", "var(--app-primary-focus-ring)"),
        new("Orange focus ring", "var(--app-orange-focus-ring)"),
        new("Tertiary focus ring", "var(--app-tertiary-focus-ring)"),
    ];

    public static IReadOnlyList<TokenOption> GetOptions(string cssVariable)
    {
        if (cssVariable.Contains("shadow", StringComparison.OrdinalIgnoreCase)
            || cssVariable.Contains("focus-ring", StringComparison.OrdinalIgnoreCase))
        {
            return ShadowTokens;
        }

        return ColorTokens;
    }

    public static IReadOnlyList<TokenOption> GetOptionsForValue(string cssVariable, string? currentValue)
    {
        var options = GetOptions(cssVariable).ToList();

        if (!string.IsNullOrWhiteSpace(currentValue)
            && options.All(o => !string.Equals(o.Value, currentValue, StringComparison.Ordinal)))
        {
            options.Insert(0, new TokenOption("Current value", currentValue));
        }

        return options;
    }
}
