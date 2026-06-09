using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TeacherFind.Mobile.Core.Abstractions;

namespace TeacherFind.Mobile.Core.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Canlı backend API adresi
            // Web, admin ve mobil aynı backend'e bağlanacak.
            _httpClient.BaseAddress = new Uri("https://sxmq5mp0-7196.euw.devtunnels.ms/");
            // Microsoft'un araya soktuğu o güvenlik onay sayfasını kodla pas geçiyoruz
            _httpClient.DefaultRequestHeaders.Add("X-Tunnel-Skip-AntiPhishing-Page", "true");
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(endpoint))
                    throw new ArgumentException("Endpoint boş olamaz.", nameof(endpoint));

                endpoint = endpoint.TrimStart('/');

                var response = await _httpClient.GetFromJsonAsync<T>(endpoint);

                return response!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Hatası: {ex.Message}");
                return default!;
            }
        }
    }
}
