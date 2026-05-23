using Microsoft.AspNetCore.Components;

namespace Client.Components.Validation;

public partial class CustomAlert : ComponentBase, IDisposable
{
    private CancellationTokenSource? _autoCloseCts;

    /// <summary>Root CSS classes: custom-alert + severity + variant + optional <see cref="Class"/>.</summary>
    private string GetAlertRootClass()
    {
        var parts = new List<string> { "custom-alert", GetSeverityClass(), GetVariantClass() };
        if (!string.IsNullOrWhiteSpace(Class))
        {
            parts.Add(Class!);
        }

        return string.Join(' ', parts.Where(static s => !string.IsNullOrEmpty(s)));
    }

    private string GetSeverityClass() => Severity switch
    {
        AlertSeverity.Success => "custom-alert--success",
        AlertSeverity.Warning => "custom-alert--warning",
        AlertSeverity.Error => "custom-alert--error",
        _ => "custom-alert--info"
    };

    private string GetVariantClass() => Variant switch
    {
        AlertVariant.Outlined => "custom-alert--outlined",
        AlertVariant.Text => "custom-alert--text",
        _ => string.Empty
    };

    private string GetIconClass() => Severity switch
    {
        AlertSeverity.Success => "bi-check-circle-fill",
        AlertSeverity.Warning => "bi-exclamation-triangle-fill",
        AlertSeverity.Error => "bi-x-circle-fill",
        _ => "bi-info-circle-fill"
    };

    /// <summary>
    /// The message to display in the alert. Ignored if ChildContent is provided.
    /// </summary>
    [Parameter]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The severity of the alert (Info, Success, Warning, Error).
    /// </summary>
    [Parameter]
    public AlertSeverity Severity { get; set; } = AlertSeverity.Info;

    /// <summary>
    /// The variant style of the alert (Text, Filled, Outlined).
    /// </summary>
    [Parameter]
    public AlertVariant Variant { get; set; } = AlertVariant.Filled;

    /// <summary>
    /// Whether the alert is currently visible.
    /// </summary>
    [Parameter]
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Event callback when visibility changes (e.g., when close button is clicked).
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    /// <summary>
    /// Event callback when the close icon is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the alert.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Custom content to display inside the alert. Overrides Message if provided.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// If true, automatically closes the alert after 5 seconds.
    /// </summary>
    [Parameter]
    public bool DelayedClose { get; set; } = false;

    protected override void OnParametersSet()
    {
        _autoCloseCts?.Cancel();
        _autoCloseCts?.Dispose();
        _autoCloseCts = null;

        if (DelayedClose && IsVisible)
        {
            _autoCloseCts = new CancellationTokenSource();
            _ = AutoCloseAsync(_autoCloseCts.Token);
        }
    }

    private async Task AutoCloseAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(5000, cancellationToken);
            if (!cancellationToken.IsCancellationRequested && IsVisible)
            {
                await HandleClose();
                StateHasChanged();
            }
        }
        catch (TaskCanceledException)
        {
            // Timer was cancelled, ignore
        }
    }

    private async Task HandleClose()
    {
        _autoCloseCts?.Cancel();

        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
    }

    /// <summary>
    /// Shows the alert.
    /// </summary>
    public async Task ShowAsync()
    {
        IsVisible = true;
        await IsVisibleChanged.InvokeAsync(true);
        StateHasChanged();
    }

    /// <summary>
    /// Hides the alert.
    /// </summary>
    public async Task HideAsync()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
        StateHasChanged();
    }

    public void Dispose()
    {
        _autoCloseCts?.Cancel();
        _autoCloseCts?.Dispose();
    }
}
