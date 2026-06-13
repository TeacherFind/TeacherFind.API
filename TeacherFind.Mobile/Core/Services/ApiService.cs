using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Storage;
using TeacherFind.Mobile.Core.Abstractions;

namespace TeacherFind.Mobile.Core.Services
{
    public class ApiService : IApiService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly string[] TokenKeys =
        {
            "accessToken",
            "access_token",
            "authToken",
            "auth_token",
            "jwtToken",
            "jwt_token",
            "token"
        };

        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress ??= new Uri("https://sxmq5mp0-7196.euw.devtunnels.ms/");

            Console.WriteLine($"API BaseAddress: {_httpClient.BaseAddress}");
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                await AttachAuthorizationHeaderAsync();

                var response = await _httpClient.GetAsync(NormalizeEndpoint(endpoint));

                if (!response.IsSuccessStatusCode)
                {
                    await LogApiErrorAsync(response);
                    return default!;
                }

                if (response.Content.Headers.ContentLength == 0)
                    return default!;

                return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Hatası: {ex.Message}");
                return default!;
            }
        }

        public async Task<bool> PutAsync<TRequest>(string endpoint, TRequest request)
        {
            try
            {
                await AttachAuthorizationHeaderAsync();

                var response = await _httpClient.PutAsJsonAsync(
                    NormalizeEndpoint(endpoint),
                    request,
                    JsonOptions);

                if (response.IsSuccessStatusCode)
                    return true;

                await LogApiErrorAsync(response);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Hatası: {ex.Message}");
                return false;
            }
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            await AttachAuthorizationHeaderAsync();

            var response = await _httpClient.PostAsJsonAsync(
                NormalizeEndpoint(endpoint),
                request,
                JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                var errorMessage = ParseErrorMessage(body) ?? $"{(int)response.StatusCode}: {body}";
                throw new Exception(errorMessage);
            }

            if (response.Content.Headers.ContentLength == 0)
                return default!;

            return (await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions))!;
        }

        public async Task<TResponse> UploadFileAsync<TResponse>(
            string endpoint,
            FileResult file,
            string formFieldName = "file")
        {
            try
            {
                await AttachAuthorizationHeaderAsync();

                await using var stream = await file.OpenReadAsync();
                using var form = new MultipartFormDataContent();
                using var content = new StreamContent(stream);

                content.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType);

                form.Add(content, formFieldName, file.FileName);

                var response = await _httpClient.PostAsync(
                    NormalizeEndpoint(endpoint),
                    form);

                if (!response.IsSuccessStatusCode)
                {
                    await LogApiErrorAsync(response);
                    return default!;
                }

                if (response.Content.Headers.ContentLength == 0)
                    return default!;

                return (await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions))!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Hatası: {ex.Message}");
                return default!;
            }
        }

        public string ToAbsoluteUrl(string? relativeOrAbsoluteUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsoluteUrl))
                return "default_avatar.png";

            if (Uri.TryCreate(relativeOrAbsoluteUrl, UriKind.Absolute, out _))
                return relativeOrAbsoluteUrl;

            var baseAddress = _httpClient.BaseAddress ?? CreateDefaultBaseAddress();

            return new Uri(
                baseAddress,
                relativeOrAbsoluteUrl.TrimStart('/')).ToString();
        }

        private static Uri CreateDefaultBaseAddress()
        {
#if ANDROID
            // Gerçek Android telefon + USB ADB reverse için.
            // CMD:
            // adb reverse tcp:5288 tcp:5288
            return new Uri("http://127.0.0.1:5288/");
#else
            // Windows Machine için.
            return new Uri("http://127.0.0.1:5288/");
#endif
        }

        private static string NormalizeEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint boş olamaz.", nameof(endpoint));

            return endpoint.TrimStart('/');
        }

        private async Task AttachAuthorizationHeaderAsync()
        {
            if (_httpClient.DefaultRequestHeaders.Authorization is not null)
                return;

            foreach (var key in TokenKeys)
            {
                var token = await SecureStorage.Default.GetAsync(key);

                if (string.IsNullOrWhiteSpace(token))
                    continue;

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                return;
            }
        }

        private static async Task LogApiErrorAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine(
                $"API Hatası: {(int)response.StatusCode} {response.ReasonPhrase} {body}");
        }

        private static string? ParseErrorMessage(string jsonBody)
        {
            if (string.IsNullOrWhiteSpace(jsonBody))
                return null;

            try
            {
                using var document = JsonDocument.Parse(jsonBody);
                var root = document.RootElement;

                // 1. Check if it's a ValidationProblemDetails (has "errors" object)
                if (root.TryGetProperty("errors", out var errorsProp) && errorsProp.ValueKind == JsonValueKind.Object)
                {
                    var errorMessages = new List<string>();
                    foreach (var property in errorsProp.EnumerateObject())
                    {
                        if (property.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var msg in property.Value.EnumerateArray())
                            {
                                errorMessages.Add(msg.GetString() ?? "");
                            }
                        }
                        else if (property.Value.ValueKind == JsonValueKind.String)
                        {
                            errorMessages.Add(property.Value.GetString() ?? "");
                        }
                    }
                    if (errorMessages.Count > 0)
                        return string.Join("\n", errorMessages);
                }

                // 2. Check if it's a generic { "message": "error text" }
                if (root.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.String)
                {
                    return messageProp.GetString();
                }

                // 3. Check for specific Title if no explicit message
                if (root.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String)
                {
                    return titleProp.GetString();
                }
            }
            catch
            {
                // Not valid JSON, ignore
            }

            return null;
        }
    }
}
