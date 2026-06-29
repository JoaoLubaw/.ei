namespace Pontuei.App.Services;

public static class AppConstants
{
    public const int MaxProgramSelection = 3;

    public static string ResolveStorageUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return path;

        return $"{AppConfig.StorageBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}