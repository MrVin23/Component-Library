using System.Timers;
using Client.Interfaces.Authorisation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Client.Utils.UserPermissions
{
    /// <summary>
    /// Service that monitors token expiration and automatically refreshes when needed.
    /// Inject as Singleton and call StartMonitoring() after successful login.
    /// </summary>
    public class TokenRefreshService : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly int _checkIntervalMs;
        private System.Timers.Timer? _timer;
        private bool _isMonitoring;

        // Events for UI notification
        public event Action? OnTokenRefreshed;
        public event Action<string>? OnTokenExpired;
        public event Action<string>? OnRefreshFailed;

        // Refresh when less than 10 minutes remaining
        private const int REFRESH_THRESHOLD_MINUTES = 10;

        public TokenRefreshService(
            IServiceProvider serviceProvider,
            IOptions<TokenRefreshOptions> options)
        {
            _serviceProvider = serviceProvider;
            var sec = options.Value.CheckIntervalSeconds;
            if (sec < 1)
                sec = 120;
            _checkIntervalMs = Math.Clamp(sec, 1, 86_400) * 1000; // 1s–24h
        }

        /// <summary>
        /// Gets a scoped IAuthService instance
        /// </summary>
        private IAuthService GetAuthService()
        {
            return _serviceProvider.GetRequiredService<IAuthService>();
        }

        /// <summary>
        /// Start monitoring token expiration. Call after successful login.
        /// </summary>
        public void StartMonitoring()
        {
            if (_isMonitoring) return;

            _timer = new System.Timers.Timer(_checkIntervalMs);
            _timer.Elapsed += async (sender, e) => await CheckAndRefreshToken();
            _timer.AutoReset = true;
            _timer.Start();
            _isMonitoring = true;
        }

        /// <summary>
        /// Stop monitoring. Call on logout.
        /// </summary>
        public void StopMonitoring()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
            _isMonitoring = false;
        }

        private async Task CheckAndRefreshToken()
        {
            try
            {
                var authService = GetAuthService();
                var statusResponse = await authService.GetTokenStatusAsync();
                
                if (statusResponse == null || !statusResponse.Success)
                {
                    // Token is invalid or expired
                    StopMonitoring();
                    OnTokenExpired?.Invoke("Session expired. Please log in again.");
                    return;
                }

                var status = statusResponse.Data;
                if (status == null)
                {
                    return;
                }

                // Check if we need to refresh (expiring within threshold)
                if (status.IsExpiringSoon)
                {
                    var refreshResponse = await authService.RefreshTokenAsync();
                    
                    if (refreshResponse?.Success == true)
                    {
                        OnTokenRefreshed?.Invoke();
                    }
                    else
                    {
                        OnRefreshFailed?.Invoke(refreshResponse?.Message ?? "Token refresh failed");
                    }
                }
            }
            catch (Exception ex)
            {
                OnRefreshFailed?.Invoke($"Error checking token: {ex.Message}");
            }
        }

        /// <summary>
        /// Manually check and refresh if needed
        /// </summary>
        public async Task<bool> CheckAndRefreshIfNeededAsync()
        {
            try
            {
                var authService = GetAuthService();
                var statusResponse = await authService.GetTokenStatusAsync();
                
                if (statusResponse?.Success != true || statusResponse.Data == null)
                {
                    return false;
                }

                if (statusResponse.Data.IsExpiringSoon)
                {
                    var refreshResponse = await authService.RefreshTokenAsync();
                    return refreshResponse?.Success == true;
                }

                return true; // Token is still valid
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            StopMonitoring();
        }
    }
}

