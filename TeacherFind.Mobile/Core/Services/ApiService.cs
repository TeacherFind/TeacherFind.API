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

        // DI Motorundan internete çıkış aracımızı (HttpClient) istiyoruz
        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Eğer backend API'nin sabit bir ana adresi varsa buraya yazabilirsin. 
            // Şimdilik yoruma aldım, ileride kendi local/sunucu API adresini buraya ekleyeceksin.
            // _httpClient.BaseAddress = new Uri("https://localhost:7001/"); 
        }

        // İŞTE EKSİK OLAN VE HATAYA SEBEP OLAN ASIL METOT:
        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                // Gelen endpoint adresine (Örn: "api/teachers") istek atıp gelen JSON'ı T (List<User>) formatına çevirir
                var response = await _httpClient.GetFromJsonAsync<T>(endpoint);
                return response;
            }
            catch (Exception ex)
            {
                // API'ye ulaşılamazsa veya hata çıkarsa program çökmesin diye hatayı yakalıyoruz
                Console.WriteLine($"API Hatası: {ex.Message}");
                return default; // Hata durumunda boş (null) döner
            }
        }
    }
}