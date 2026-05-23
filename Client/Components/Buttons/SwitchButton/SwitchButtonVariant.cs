namespace Client.Components.Buttons.SwitchButton;

/// <summary>
/// Visual and behavioral mode for <see cref="CustomSwitchButton"/>.
/// </summary>
public enum SwitchButtonVariant
{
    /// <summary>
    /// Standard toggle track and thumb (no theme icons).
    /// </summary>
    Normal,

    /// <summary>
    /// Theme toggle with sun and moon icons; drives <c>html[data-theme]</c> when bound state changes.
    /// </summary>
    DarkLightMode,
}
