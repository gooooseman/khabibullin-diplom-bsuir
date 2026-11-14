using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FinancialAssistant.Core.Interfaces;
using Microsoft.Maui.Authentication;

namespace FinancialAssistant.Application.Services;

public class GoogleAuthService : IGoogleAuthService
{
    public async Task<(string? Email, string? DisplayName, string? Error)> AuthenticateAsync()
    {
        try
        {
            // Generate PKCE
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);

            var authUrl = new Uri(
                "https://accounts.google.com/o/oauth2/v2/auth" +
                $"?client_id={Uri.EscapeDataString(GoogleAuthConstants.ClientId)}" +
                $"&redirect_uri={Uri.EscapeDataString(GoogleAuthConstants.RedirectUri)}" +
                $"&response_type=code" +
                $"&scope={Uri.EscapeDataString("email profile openid")}" +
                $"&code_challenge={codeChallenge}" +
                $"&code_challenge_method=S256" +
                $"&access_type=online");

            var callbackUrl = new Uri(GoogleAuthConstants.RedirectUri);

            WebAuthenticatorResult result;
            try
            {
                result = await WebAuthenticator.Default.AuthenticateAsync(
                    new WebAuthenticatorOptions
                    {
                        Url = authUrl,
                        CallbackUrl = callbackUrl,
                        PrefersEphemeralWebBrowserSession = true
                    });
            }
            catch (TaskCanceledException)
            {
                return (null, null, "Вход через Google отменён");
            }

            if (!result.Properties.TryGetValue("code", out var code) || string.IsNullOrEmpty(code))
                return (null, null, "Не удалось получить код авторизации");

            // Exchange code for tokens using PKCE (no client_secret needed for Android client)
            var (accessToken, idToken, tokenError) = await ExchangeCodeAsync(code, codeVerifier);
            if (tokenError != null)
                return (null, null, tokenError);

            // Get user info
            if (!string.IsNullOrEmpty(accessToken))
            {
                var (email, name) = await GetUserInfoAsync(accessToken);
                if (!string.IsNullOrEmpty(email))
                    return (email, name ?? email.Split('@')[0], null);
            }

            if (!string.IsNullOrEmpty(idToken))
            {
                var (email, name) = DecodeJwtClaims(idToken);
                if (!string.IsNullOrEmpty(email))
                    return (email, name ?? email.Split('@')[0], null);
            }

            return (null, null, "Не удалось получить данные из Google аккаунта");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GoogleAuth] Error: {ex.Message}");
            return (null, null, $"Вход через Google не выполнен: {ex.Message}");
        }
    }

    private static async Task<(string? AccessToken, string? IdToken, string? Error)> ExchangeCodeAsync(
        string code, string codeVerifier)
    {
        try
        {
            using var http = new System.Net.Http.HttpClient();
            var response = await http.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["code"]          = code,
                    ["client_id"]     = GoogleAuthConstants.ClientId,
                    ["redirect_uri"]  = GoogleAuthConstants.RedirectUri,
                    ["grant_type"]    = "authorization_code",
                    ["code_verifier"] = codeVerifier
                }));

            var json = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[GoogleAuth] Token response: {json}");

            if (!response.IsSuccessStatusCode)
                return (null, null, $"Ошибка обмена токена: {response.StatusCode}");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var accessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() : null;
            var idToken = root.TryGetProperty("id_token", out var it) ? it.GetString() : null;
            return (accessToken, idToken, null);
        }
        catch (Exception ex)
        {
            return (null, null, $"Ошибка обмена токена: {ex.Message}");
        }
    }

    private static async Task<(string? Email, string? Name)> GetUserInfoAsync(string accessToken)
    {
        try
        {
            using var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.GetStringAsync("https://www.googleapis.com/oauth2/v3/userinfo");
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            root.TryGetProperty("email", out var emailEl);
            root.TryGetProperty("name", out var nameEl);
            return (
                emailEl.ValueKind == JsonValueKind.String ? emailEl.GetString() : null,
                nameEl.ValueKind == JsonValueKind.String ? nameEl.GetString() : null
            );
        }
        catch { return (null, null); }
    }

    private static (string? Email, string? Name) DecodeJwtClaims(string idToken)
    {
        try
        {
            var parts = idToken.Split('.');
            if (parts.Length < 2) return (null, null);
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            root.TryGetProperty("email", out var emailEl);
            root.TryGetProperty("name", out var nameEl);
            return (
                emailEl.ValueKind == JsonValueKind.String ? emailEl.GetString() : null,
                nameEl.ValueKind == JsonValueKind.String ? nameEl.GetString() : null
            );
        }
        catch { return (null, null); }
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeChallenge(string verifier)
    {
        var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
