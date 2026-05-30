using Client.Components.Buttons.CustomButton;

namespace Client.ScrollMaster.ButtonDesigner.Models;

public static class ButtonCssVarSchema
{
    public static readonly CustomButtonType[] SupportedButtonTypes =
    [
        CustomButtonType.Primary,
        CustomButtonType.Secondary,
        CustomButtonType.Tertiary
    ];

    private static readonly string[] DefaultKeys =
    [
        "--btn-fg",
        "--btn-bg",
        "--btn-border"
    ];

    private static readonly string[] HoverKeys =
    [
        "--btn-hover-fg",
        "--btn-hover-bg",
        "--btn-hover-border",
        "--btn-shadow-lift"
    ];

    private static readonly string[] ActiveKeys =
    [
        "--btn-active-fg",
        "--btn-active-bg",
        "--btn-active-border"
    ];

    private static readonly string[] FocusKeys = ["--btn-focus-ring"];

    public static Dictionary<ButtonStyleState, Dictionary<string, string>> CreateEmptyVariableStore()
    {
        return new Dictionary<ButtonStyleState, Dictionary<string, string>>
        {
            [ButtonStyleState.Default] = CreateDictionary(DefaultKeys),
            [ButtonStyleState.Hover] = CreateDictionary(HoverKeys),
            [ButtonStyleState.Active] = CreateDictionary(ActiveKeys),
            [ButtonStyleState.Focus] = CreateDictionary(FocusKeys),
            [ButtonStyleState.Disabled] = new Dictionary<string, string>(),
            [ButtonStyleState.Constant] = new Dictionary<string, string>()
        };
    }

    public static IReadOnlyList<string> GetKeys(ButtonStyleState state) => state switch
    {
        ButtonStyleState.Default => DefaultKeys,
        ButtonStyleState.Hover => HoverKeys,
        ButtonStyleState.Active => ActiveKeys,
        ButtonStyleState.Focus => FocusKeys,
        _ => Array.Empty<string>()
    };

    public static void ApplyDefaults(DesignedButton button)
    {
        var defaults = GetDefaultsForType(button.ButtonType);
        foreach (var (state, values) in defaults)
        {
            foreach (var (key, value) in values)
                button.Variables[state][key] = value;
        }
    }

    public static void RefreshTypeDefaults(DesignedButton button)
    {
        ApplyDefaults(button);
    }

    private static Dictionary<ButtonStyleState, Dictionary<string, string>> GetDefaultsForType(
        CustomButtonType type) =>
        type switch
        {
            CustomButtonType.Secondary => SecondaryDefaults(),
            CustomButtonType.Tertiary => TertiaryDefaults(),
            _ => PrimaryDefaults()
        };

    private static Dictionary<ButtonStyleState, Dictionary<string, string>> PrimaryDefaults() => new()
    {
        [ButtonStyleState.Default] = CreateDictionary(DefaultKeys, new Dictionary<string, string>
        {
            ["--btn-fg"] = "var(--app-primary)",
            ["--btn-bg"] = "var(--app-primary-soft-bg)",
            ["--btn-border"] = "var(--app-primary-soft-border)"
        }),
        [ButtonStyleState.Hover] = CreateDictionary(HoverKeys, new Dictionary<string, string>
        {
            ["--btn-hover-fg"] = "var(--text-color-inverse)",
            ["--btn-hover-bg"] = "var(--app-primary-hover-bg)",
            ["--btn-hover-border"] = "var(--app-primary-hover-bg)",
            ["--btn-shadow-lift"] = "var(--app-primary-shadow-lift)"
        }),
        [ButtonStyleState.Active] = CreateDictionary(ActiveKeys, new Dictionary<string, string>
        {
            ["--btn-active-fg"] = "var(--text-color-inverse)",
            ["--btn-active-bg"] = "var(--app-primary-active-bg)",
            ["--btn-active-border"] = "var(--app-primary-active-bg)"
        }),
        [ButtonStyleState.Focus] = CreateDictionary(FocusKeys, new Dictionary<string, string>
        {
            ["--btn-focus-ring"] = "var(--app-primary-focus-ring)"
        })
    };

    private static Dictionary<ButtonStyleState, Dictionary<string, string>> SecondaryDefaults() => new()
    {
        [ButtonStyleState.Default] = CreateDictionary(DefaultKeys, new Dictionary<string, string>
        {
            ["--btn-fg"] = "var(--app-orange-fg)",
            ["--btn-bg"] = "var(--app-orange-soft-bg)",
            ["--btn-border"] = "var(--app-orange-soft-border)"
        }),
        [ButtonStyleState.Hover] = CreateDictionary(HoverKeys, new Dictionary<string, string>
        {
            ["--btn-hover-fg"] = "var(--app-orange-hover-fg)",
            ["--btn-hover-bg"] = "var(--app-orange-hover-bg)",
            ["--btn-hover-border"] = "var(--app-orange-hover-bg)",
            ["--btn-shadow-lift"] = "var(--app-orange-shadow-lift)"
        }),
        [ButtonStyleState.Active] = CreateDictionary(ActiveKeys, new Dictionary<string, string>
        {
            ["--btn-active-fg"] = "var(--app-orange-active-fg)",
            ["--btn-active-bg"] = "var(--app-orange-active-bg)",
            ["--btn-active-border"] = "var(--app-orange-active-bg)"
        }),
        [ButtonStyleState.Focus] = CreateDictionary(FocusKeys, new Dictionary<string, string>
        {
            ["--btn-focus-ring"] = "var(--app-orange-focus-ring)"
        })
    };

    private static Dictionary<ButtonStyleState, Dictionary<string, string>> TertiaryDefaults() => new()
    {
        [ButtonStyleState.Default] = CreateDictionary(DefaultKeys, new Dictionary<string, string>
        {
            ["--btn-fg"] = "var(--app-tertiary-fg)",
            ["--btn-bg"] = "var(--app-tertiary-soft-bg)",
            ["--btn-border"] = "var(--app-tertiary-soft-border)"
        }),
        [ButtonStyleState.Hover] = CreateDictionary(HoverKeys, new Dictionary<string, string>
        {
            ["--btn-hover-fg"] = "var(--app-tertiary-hover-fg)",
            ["--btn-hover-bg"] = "var(--app-tertiary-hover-bg)",
            ["--btn-hover-border"] = "var(--app-tertiary-hover-bg)",
            ["--btn-shadow-lift"] = "var(--app-tertiary-shadow-lift)"
        }),
        [ButtonStyleState.Active] = CreateDictionary(ActiveKeys, new Dictionary<string, string>
        {
            ["--btn-active-fg"] = "var(--app-tertiary-active-fg)",
            ["--btn-active-bg"] = "var(--app-tertiary-active-bg)",
            ["--btn-active-border"] = "var(--app-tertiary-active-bg)"
        }),
        [ButtonStyleState.Focus] = CreateDictionary(FocusKeys, new Dictionary<string, string>
        {
            ["--btn-focus-ring"] = "var(--app-tertiary-focus-ring)"
        })
    };

    private static Dictionary<string, string> CreateDictionary(
        IEnumerable<string> keys,
        Dictionary<string, string>? seed = null)
    {
        var dict = new Dictionary<string, string>();
        foreach (var key in keys)
            dict[key] = seed != null && seed.TryGetValue(key, out var value) ? value : string.Empty;
        return dict;
    }
}
