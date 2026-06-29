using Pontuei.Shared.Dtos.Responses;

namespace Pontuei.App.Services;

/// <summary>
/// True singleton service that manages the authentication state of the app:
/// - Persists the access token, refresh token, and current user ID in SecureStorage.
/// - Exposes static properties so that ApiClient can inject the Bearer token without needing DI
/// </summary>
public static class AuthService
{
    private const string KeyAccessToken = "auth_access_token";
    private const string KeyRefreshToken = "auth_refresh_token";
    private const string KeyUserId = "auth_user_id";
    private const string KeySessionId = "auth_session_id";


    // ── Memory state (loaded once at startup) ──────────────────
    public static string? AccessToken { get; private set; }
    public static string? RefreshToken { get; private set; }
    public static Guid? CurrentUserId { get; private set; }
    public static Guid? CurrentSessionId { get; private set; }

    public static bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    // ── Initialization ─────────────────

    /// <summary>
    /// Loads the tokens and user ID from SecureStorage into memory.
    /// Should be called before any navigation.
    /// </summary>
    public static async Task InitializeAsync()
    {
        AccessToken = await SecureStorage.Default.GetAsync(KeyAccessToken);
        RefreshToken = await SecureStorage.Default.GetAsync(KeyRefreshToken);

        string? rawUserId = await SecureStorage.Default.GetAsync(KeyUserId);
        CurrentUserId = Guid.TryParse(rawUserId, out Guid id) ? id : null;
    }

    // ── Persistence (after login/refresh) ─────────────────────────────────

    /// <summary>
    /// Save the returned tokens to SecureStorage and memory.
    /// </summary>
    public static async Task SaveSessionAsync(LoginResponseDto loginResponse)
    {
        AccessToken = loginResponse.AccessToken;
        RefreshToken = loginResponse.RefreshToken;
        CurrentUserId = loginResponse.User.UserId;
        CurrentSessionId = loginResponse.SessionId;

        await SecureStorage.Default.SetAsync(KeyAccessToken, loginResponse.AccessToken);
        await SecureStorage.Default.SetAsync(KeyRefreshToken, loginResponse.RefreshToken);
        await SecureStorage.Default.SetAsync(KeyUserId, loginResponse.User.UserId.ToString());
        await SecureStorage.Default.SetAsync(KeySessionId, loginResponse.SessionId.ToString());
    }

    // ── Logout ────────────────────────────────────────────────────────────

    /// <summary>
    /// Clears the tokens from memory and SecureStorage.
    /// </summary>
    public static async Task LogoutAsync()
    {
        AccessToken = null;
        RefreshToken = null;
        CurrentUserId = null;

        SecureStorage.Default.Remove(KeyAccessToken);
        SecureStorage.Default.Remove(KeyRefreshToken);
        SecureStorage.Default.Remove(KeyUserId);

        await Task.CompletedTask;
    }
}