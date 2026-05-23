using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace Client.Components.Containers.FormContainer;

public partial class FormContainer : ComponentBase, IDisposable
{
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = new();

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>Optional heading shown when <see cref="ShowTitle"/> is true.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>When false, the title block is not rendered.</summary>
    [Parameter]
    public bool ShowTitle { get; set; } = true;

    /// <summary>When true, a Reset action is shown in the bottom-right action row.</summary>
    [Parameter]
    public bool ShowResetButton { get; set; }

    /// <summary>Label for the reset/secondary action (default <c>Reset</c>; use e.g. <c>Cancel</c> when leaving edit mode).</summary>
    [Parameter]
    public string ResetButtonText { get; set; } = "Reset";

    /// <summary>When true, a Save action is shown in the bottom-right action row.</summary>
    [Parameter]
    public bool ShowSaveButton { get; set; }

    /// <summary>Label for the save/primary action (default <c>Save</c>).</summary>
    [Parameter]
    public string SaveButtonText { get; set; } = "Save";

    [Parameter]
    public bool ResetDisabled { get; set; }

    [Parameter]
    public bool SaveDisabled { get; set; }

    /// <summary>HTML type for the reset control (e.g. <c>reset</c> inside a native <c>form</c>).</summary>
    [Parameter]
    public string ResetButtonHtmlType { get; set; } = "button";

    /// <summary>HTML type for the save control (use <c>submit</c> when this container sits inside <c>EditForm</c>).</summary>
    [Parameter]
    public string SaveButtonHtmlType { get; set; } = "button";

    [Parameter]
    public EventCallback<MouseEventArgs> OnReset { get; set; }

    [Parameter]
    public EventCallback<MouseEventArgs> OnSave { get; set; }

    /// <summary>
    /// When true and the component is under an <see cref="EditForm"/>, validation messages from the cascaded
    /// <see cref="EditContext"/> are shown in a <c>CustomAlert</c>.
    /// </summary>
    [Parameter]
    public bool ShowValidationSummary { get; set; }

    [CascadingParameter]
    public EditContext? CascadedEditContext { get; set; }

    protected bool HasActionRow => ShowResetButton || ShowSaveButton;

    protected bool HasTitleBlock => ShowTitle && !string.IsNullOrWhiteSpace(Title);

    protected bool ShowValidationSummaryBlock =>
        ShowValidationSummary && CascadedEditContext is not null;

    protected bool ValidationSummaryAlertVisible =>
        ShowValidationSummaryBlock && _validationMessages.Count > 0 && !_validationSummaryDismissed;

    protected IReadOnlyList<string> ValidationSummaryMessages => _validationMessages;

    private EditContext? _subscribedContext;
    private readonly List<string> _validationMessages = new();
    private string _validationFingerprint = string.Empty;
    private bool _validationSummaryDismissed;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (!ShowValidationSummary || CascadedEditContext is null)
        {
            DetachValidationListener();
            ClearValidationMessages();
            return;
        }

        if (!ReferenceEquals(_subscribedContext, CascadedEditContext))
        {
            DetachValidationListener();
            _subscribedContext = CascadedEditContext;
            _subscribedContext.OnValidationStateChanged += OnValidationStateChanged;
        }

        RefreshValidationMessages();
    }

    private void OnValidationStateChanged(object? sender, ValidationStateChangedEventArgs e) =>
        RefreshValidationMessages();

    private void RefreshValidationMessages()
    {
        if (_subscribedContext is null)
        {
            ClearValidationMessages();
            return;
        }

        _validationMessages.Clear();
        _validationMessages.AddRange(_subscribedContext.GetValidationMessages().Distinct());

        var fingerprint = string.Join("\u001e", _validationMessages.OrderBy(m => m, StringComparer.Ordinal));
        if (!string.Equals(fingerprint, _validationFingerprint, StringComparison.Ordinal))
        {
            _validationFingerprint = fingerprint;
            _validationSummaryDismissed = false;
        }

        if (_validationMessages.Count == 0)
        {
            _validationFingerprint = string.Empty;
            _validationSummaryDismissed = false;
        }

        _ = InvokeAsync(StateHasChanged);
    }

    private void ClearValidationMessages()
    {
        _validationMessages.Clear();
        _validationFingerprint = string.Empty;
        _validationSummaryDismissed = false;
    }

    private void DetachValidationListener()
    {
        if (_subscribedContext is not null)
        {
            _subscribedContext.OnValidationStateChanged -= OnValidationStateChanged;
            _subscribedContext = null;
        }
    }

    public void Dispose() => DetachValidationListener();

    private void HandleValidationSummaryClose()
    {
        _validationSummaryDismissed = true;
        StateHasChanged();
    }

    private Task HandleResetAsync(MouseEventArgs args) => OnReset.InvokeAsync(args);

    private Task HandleSaveAsync(MouseEventArgs args) => OnSave.InvokeAsync(args);
}
