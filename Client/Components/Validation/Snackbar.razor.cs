using Microsoft.AspNetCore.Components;

namespace Client.Components.Validation;

public partial class Snackbar : ComponentBase, IDisposable
{
    private AlertSeverity _severity = AlertSeverity.Info;
    private string? _titleOverride;
    private System.Threading.Timer? _autoHideTimer;

    [Parameter]
    public string Message { get; set; } = string.Empty;

    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    /// <summary>Optional header title when visible. If null, a default label is chosen from severity.</summary>
    [Parameter]
    public string? HeaderTitle { get; set; }

    private string SnackbarRootClass => $"snackbar snackbar--{_severity.ToString().ToLowerInvariant()}";

    private string IconClass => _severity switch
    {
        AlertSeverity.Success => "bi-check-circle-fill",
        AlertSeverity.Warning => "bi-exclamation-triangle-fill",
        AlertSeverity.Error => "bi-x-circle-fill",
        _ => "bi-info-circle-fill"
    };

    private string EffectiveHeaderTitle =>
        !string.IsNullOrWhiteSpace(_titleOverride)
            ? _titleOverride!
            : !string.IsNullOrWhiteSpace(HeaderTitle)
                ? HeaderTitle!
                : DefaultTitleForSeverity(_severity);

    private static string DefaultTitleForSeverity(AlertSeverity severity) => severity switch
    {
        AlertSeverity.Success => "Success",
        AlertSeverity.Warning => "Warning",
        AlertSeverity.Error => "Error",
        _ => "Information"
    };

    private async Task HandleClose()
    {
        IsVisible = false;
        _autoHideTimer?.Dispose();
        _autoHideTimer = null;
        _titleOverride = null;
        await IsVisibleChanged.InvokeAsync(IsVisible);
        StateHasChanged();
    }

    /// <summary>Show the snackbar with a severity (colors match <see cref="CustomAlert"/> support tokens).</summary>
    /// <param name="message">Body text.</param>
    /// <param name="severity">Visual tone.</param>
    /// <param name="title">Optional header title; default is derived from severity.</param>
    public void Show(string message, AlertSeverity severity = AlertSeverity.Info, string? title = null)
    {
        Message = message;
        _severity = severity;
        _titleOverride = title;
        IsVisible = true;

        _autoHideTimer?.Dispose();
        _autoHideTimer = new System.Threading.Timer(async _ =>
        {
            await InvokeAsync(async () => { await HandleClose(); });
        }, null, 5000, Timeout.Infinite);

        StateHasChanged();
    }

    public void Hide()
    {
        _autoHideTimer?.Dispose();
        _autoHideTimer = null;
        IsVisible = false;
        _titleOverride = null;
        StateHasChanged();
    }

    public void Dispose()
    {
        _autoHideTimer?.Dispose();
    }
}
