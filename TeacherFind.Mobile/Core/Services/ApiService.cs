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
            _httpClient.BaseAddress ??= new Uri("http://127.0.0.1:5288/");
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

                var response = await _httpClient.PostAsync(NormalizeEndpoint(endpoint), form);
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

            var baseAddress = _httpClient.BaseAddress ?? new Uri("https://localhost:7196/");
            return new Uri(baseAddress, relativeOrAbsoluteUrl.TrimStart('/')).ToString();
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
            Console.WriteLine($"API Hatası: {(int)response.StatusCode} {response.ReasonPhrase} {body}");
        }
    }
}
