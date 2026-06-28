namespace Pontuei.App.Services;

public static class AppConstants
{
    /// <summary>
    /// Base pública do MinIO para logos dos programas.
    /// Android emulator: 10.0.2.2 aponta para localhost da máquina host.
    /// </summary>
#if ANDROID
    public const string StorageBaseUrl = "http://10.0.2.2:9000";
#elif IOS || MACCATALYST
    public const string StorageBaseUrl = "http://localhost:9000";
#else
    public const string StorageBaseUrl = "http://localhost:9000";
#endif

    public const int MaxProgramSelection = 3;

    public static string ResolveStorageUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return path;

        return $"{StorageBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}