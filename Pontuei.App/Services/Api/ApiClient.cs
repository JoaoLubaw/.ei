using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Responses;

namespace Pontuei.App.Services.Api;

/// <summary>
/// Central Wrapper to HttpClient for API calls.
/// Ijects the access token into the Authorization header and handles errors in a standardized way.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    // ── Public Methods ─────────────────────────────────

    public Task<ApiResponse<T>> GetAsync<T>(string url)
        => SendAsync<T>(HttpMethod.Get, url);

    public Task<ApiResponse<T>> PostAsync<T>(string url, object? body = null, bool suppressToast = false)
        => SendAsync<T>(HttpMethod.Post, url, body, suppressToast);

    public Task<ApiResponse<T>> PatchAsync<T>(string url, object? body = null)
        => SendAsync<T>(HttpMethod.Patch, url, body);

    public Task<ApiResponse<T>> PutAsync<T>(string url, object? body = null)
        => SendAsync<T>(HttpMethod.Put, url, body);

    public Task<ApiResponse<T>> DeleteAsync<T>(string url)
        => SendAsync<T>(HttpMethod.Delete, url);

    /// <summary>
    /// Sends a multipart/form-data request (used for uploading transaction media).
    /// </summary>
    public Task<ApiResponse<T>> PostMultipartAsync<T>(string url, MultipartFormDataContent content)
        => SendMultipartAsync<T>(HttpMethod.Post, url, content);

    public Task<ApiResponse<T>> PutMultipartAsync<T>(string url, MultipartFormDataContent content)
        => SendMultipartAsync<T>(HttpMethod.Put, url, content);

    // ── Private Implementations ───────────────────────────────────────────

    private async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string url, object? body = null, bool suppressToast = false)
    {
        using HttpRequestMessage request = new(method, url);
        AttachToken(request);

        if (body != null)
        {
            string json = JsonSerializer.Serialize(body, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return await ExecuteAsync<T>(request, suppressToast);
    }

    private async Task<ApiResponse<T>> SendMultipartAsync<T>(HttpMethod method, string url, MultipartFormDataContent content)
    {
        using HttpRequestMessage request = new(method, url) { Content = content };
        AttachToken(request);
        return await ExecuteAsync<T>(request);
    }

    private void AttachToken(HttpRequestMessage request)
    {
        if (request.RequestUri?.OriginalString.Contains("/auth/") == true)
            return;

        string? token = AuthService.AccessToken;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<ApiResponse<T>> ExecuteAsync<T>(HttpRequestMessage request, bool suppressToast = false)
    {
        try
        {
            HttpResponseMessage response = await _http.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ApiResponse<LoginResponseDto> refreshResult = await new AuthApiService(this).RefreshTokenAsync();
                if (refreshResult.IsSuccess)
                {
                    return await SendAsync<T>(request.Method, request.RequestUri.ToString(),
                        request.Content is StringContent sc ? JsonSerializer.Deserialize<object>(await sc.ReadAsStringAsync()) : null
                    );
                }
                else
                {
                    await HandleUnauthorizedAsync();

                    return ApiResponse<T>.Fail(HttpStatusCode.Unauthorized, "Sessão expirada. Faça login novamente.");
                }
            }

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent || string.IsNullOrWhiteSpace(responseBody))
                    return ApiResponse<T>.Ok(default!);

                T? data = JsonSerializer.Deserialize<T>(responseBody, _jsonOptions);
                return ApiResponse<T>.Ok(data!);
            }

            string errorMessage = TryExtractErrorMessage(responseBody)
                ?? $"Erro {(int)response.StatusCode}.";

            if (!suppressToast)
                await Toast.Make(errorMessage, ToastDuration.Long, 14).Show();

            return ApiResponse<T>.Fail(response.StatusCode, errorMessage);
        }
        catch (TaskCanceledException)
        {
            string msg = "Tempo de resposta esgotado. Verifique sua conexão.";
            await Toast.Make(msg, ToastDuration.Long, 14).Show();
            return ApiResponse<T>.Fail(HttpStatusCode.RequestTimeout, msg);
        }
        catch (HttpRequestException)
        {
            string msg = "Não foi possível conectar ao servidor. Verifique sua conexão.";
            await Toast.Make(msg, ToastDuration.Long, 14).Show();
            return ApiResponse<T>.Fail(HttpStatusCode.ServiceUnavailable, msg);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ApiClient] Unexpected error: {ex}");
            string msg = "Ocorreu um erro inesperado. Tente novamente mais tarde.";
            await Toast.Make(msg, ToastDuration.Long, 14).Show();
            return ApiResponse<T>.Fail(HttpStatusCode.InternalServerError, msg);
        }
    }

    private static string? TryExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using JsonDocument doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("message", out JsonElement msg))
                return msg.GetString();
        }
        catch { /* body is not a valid JSON */ }
        return null;
    }

    private static async Task HandleUnauthorizedAsync()
    {
        await AuthService.LogoutAsync();

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync("//auth");
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Serializes an object into a query string, appending it to the base URL.
    /// Ignores null values and uses property names as query parameter keys.
    /// </summary>
    public static string BuildQueryString(string baseUrl, object? queryObject)
    {
        if (queryObject is null) return baseUrl;

        IEnumerable<string> props = queryObject.GetType().GetProperties()
            .Where(p => p.GetValue(queryObject) != null)
            .Select(p => $"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(p.GetValue(queryObject)!.ToString()!)}");

        string qs = string.Join("&", props);
        return string.IsNullOrEmpty(qs) ? baseUrl : $"{baseUrl}?{qs}";
    }

}

/// <summary>
/// Response wrapper for API calls, providing success status, data, HTTP status code, and error message.
/// </summary>
public class ApiResponse<T>
{
    public bool IsSuccess { get; private init; }
    public T? Data { get; private init; }
    public HttpStatusCode StatusCode { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static ApiResponse<T> Ok(T data) => new()
    {
        IsSuccess = true,
        Data = data,
        StatusCode = HttpStatusCode.OK
    };

    public static ApiResponse<T> Fail(HttpStatusCode code, string message) => new()
    {
        IsSuccess = false,
        StatusCode = code,
        ErrorMessage = message
    };
}