using System.Text.Json;

namespace Pontuei.App;

/// <summary>
/// Loads configuration from appsettings.Local.json (gitignored).
/// Falls back to empty strings if the file or key is missing.
/// </summary>
public static class AppConfig
{
    private static JsonDocument? _doc;

    public static void Initialize()
    {
        try
        {
            using Stream stream = FileSystem.OpenAppPackageFileAsync("appsettings.Local.json").Result;
            using StreamReader reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            _doc = JsonDocument.Parse(json);
        }
        catch
        {
            System.Diagnostics.Debug.WriteLine("[AppConfig] appsettings.Local.json not found.");
        }
    }

    public static string ApiBaseUrl => Get("Api", "BaseUrl");
    public static string StorageBaseUrl => Get("Storage", "BaseUrl");
    public static string GoogleWebClientId => Get("Google", "WebClientId");

    private static string Get(string section, string key)
    {
        try
        {
            return _doc?.RootElement
                .GetProperty(section)
                .GetProperty(key)
                .GetString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}