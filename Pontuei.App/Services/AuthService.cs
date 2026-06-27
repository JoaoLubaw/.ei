namespace Pontuei.App.Services;

/// <summary>
/// Gerencia o estado de autenticação local do usuário.
/// Persiste o JWT e dados da sessão via SecureStorage (criptografado pelo SO).
/// </summary>
public static class AuthService
{
    // ── Chaves do SecureStorage ───────────────────────────────────────────
    private const string KeyAccessToken = "auth_access_token";
    private const string KeyRefreshToken = "auth_refresh_token";
    private const string KeyUserId = "auth_user_id";
    private const string KeyUserName = "auth_user_name";
    private const string KeyUserEmail = "auth_user_email";

    // ── Propriedades em memória (cache da sessão atual) ───────────────────

    public static string? AccessToken { get; private set; }
    public static string? RefreshToken { get; private set; }
    public static Guid UserId { get; private set; }
    public static string? UserName { get; private set; }
    public static string? UserEmail { get; private set; }

    // ── Inicialização ─────────────────────────────────────────────────────

    /// <summary>
    /// Carrega a sessão do SecureStorage para a memória.
    /// Chamar no startup do app (MauiProgram ou App.xaml.cs).
    /// </summary>
    public static async Task InitializeAsync()
    {
        try
        {
            AccessToken = await SecureStorage.Default.GetAsync(KeyAccessToken);
            RefreshToken = await SecureStorage.Default.GetAsync(KeyRefreshToken);
            UserName = await SecureStorage.Default.GetAsync(KeyUserName);
            UserEmail = await SecureStorage.Default.GetAsync(KeyUserEmail);

            string? rawId = await SecureStorage.Default.GetAsync(KeyUserId);
            if (Guid.TryParse(rawId, out Guid id))
                UserId = id;
        }
        catch
        {
            // SecureStorage pode falhar em emuladores sem keystore configurado.
            // Nesse caso, o usuário vai precisar fazer login novamente.
            ClearMemory();
        }
    }

    // ── Verificação ───────────────────────────────────────────────────────

    /// <summary>Retorna true se há um token de acesso salvo (sessão ativa).</summary>
    public static async Task<bool> IsLoggedInAsync()
    {
        if (AccessToken != null) return true;

        // Tenta carregar do storage caso não tenha sido inicializado ainda
        await InitializeAsync();
        return !string.IsNullOrEmpty(AccessToken);
    }

    // ── Persistência ──────────────────────────────────────────────────────

    /// <summary>
    /// Salva os dados de sessão após login bem-sucedido.
    /// </summary>
    public static async Task SaveSessionAsync(
        string accessToken,
        string refreshToken,
        Guid userId,
        string userName,
        string userEmail)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        UserId = userId;
        UserName = userName;
        UserEmail = userEmail;

        await SecureStorage.Default.SetAsync(KeyAccessToken, accessToken);
        await SecureStorage.Default.SetAsync(KeyRefreshToken, refreshToken);
        await SecureStorage.Default.SetAsync(KeyUserId, userId.ToString());
        await SecureStorage.Default.SetAsync(KeyUserName, userName);
        await SecureStorage.Default.SetAsync(KeyUserEmail, userEmail);
    }

    /// <summary>
    /// Remove todos os dados de sessão (logout).
    /// </summary>
    public static void ClearSession()
    {
        ClearMemory();

        SecureStorage.Default.Remove(KeyAccessToken);
        SecureStorage.Default.Remove(KeyRefreshToken);
        SecureStorage.Default.Remove(KeyUserId);
        SecureStorage.Default.Remove(KeyUserName);
        SecureStorage.Default.Remove(KeyUserEmail);
    }

    // ── Helpers privados ──────────────────────────────────────────────────

    private static void ClearMemory()
    {
        AccessToken = null;
        RefreshToken = null;
        UserId = Guid.Empty;
        UserName = null;
        UserEmail = null;
    }
}
