using Client.Components.Buttons.CustomButton;

namespace Client.ScrollMaster.ButtonDesigner.Models;

public sealed class DesignedButton
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Label { get; set; } = "Button";

    public string Text { get; set; } = "Button";

    public string Width { get; set; } = "auto";

    public string Height { get; set; } = "auto";

    public CustomButtonSize Size { get; set; } = CustomButtonSize.Medium;

    public CustomButtonType ButtonType { get; set; } = CustomButtonType.Primary;

    public Dictionary<ButtonStyleState, Dictionary<string, string>> Variables { get; } =
        ButtonCssVarSchema.CreateEmptyVariableStore();

    public static DesignedButton CreateNew(int index)
    {
        var button = new DesignedButton
        {
            Label = $"Button {index}",
            Text = $"Button {index}"
        };

        ButtonCssVarSchema.ApplyDefaults(button);
        return button;
    }

    public void ResetToDefaults()
    {
        Width = "auto";
        Height = "auto";
        Size = CustomButtonSize.Medium;
        ButtonType = CustomButtonType.Primary;
        ButtonCssVarSchema.ApplyDefaults(this);
    }
}
