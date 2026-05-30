namespace Client.ScrollMaster.ButtonDesigner.Models;

public static class ButtonPreviewStyleBuilder
{
    /// <summary>
    /// Applies all button CSS variables so native :hover/:active/:focus-visible work in the preview.
    /// </summary>
    public static string BuildFullPreviewStyle(DesignedButton button)
    {
        var merged = new Dictionary<string, string>();

        foreach (var state in new[]
                 {
                     ButtonStyleState.Default,
                     ButtonStyleState.Hover,
                     ButtonStyleState.Active,
                     ButtonStyleState.Focus
                 })
        {
            foreach (var pair in button.Variables[state])
            {
                if (!string.IsNullOrWhiteSpace(pair.Value))
                    merged[pair.Key] = pair.Value;
            }
        }

        return ToInlineStyle(merged);
    }

    private static string ToInlineStyle(Dictionary<string, string> variables)
    {
        var parts = variables
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
            .Select(pair => $"{pair.Key}: {pair.Value};");

        return string.Join(" ", parts);
    }
}
