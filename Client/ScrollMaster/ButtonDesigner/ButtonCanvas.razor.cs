using Client.ScrollMaster.ButtonDesigner.Models;
using Microsoft.AspNetCore.Components;

namespace Client.ScrollMaster.ButtonDesigner;

public partial class ButtonCanvas : ComponentBase
{
    private readonly List<DesignedButton> _buttons = [];
    private Guid? _selectedId;

    private DesignedButton? SelectedButton =>
        _selectedId is null
            ? null
            : _buttons.FirstOrDefault(b => b.Id == _selectedId);

    private void AddButton()
    {
        var button = DesignedButton.CreateNew(_buttons.Count + 1);
        _buttons.Add(button);
        _selectedId = button.Id;
    }

    private void SelectButton(DesignedButton button)
    {
        _selectedId = button.Id;
    }

    private void OnDesignerChanged() => StateHasChanged();
}
