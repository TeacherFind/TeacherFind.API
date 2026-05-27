using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TeacherFind.Mobile.Core.Abstractions;
using TeacherFind.Mobile.Core.Models;

namespace TeacherFind.Mobile.Core.Services;

public class ApiService : IApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient GetClient() => _httpClientFactory.CreateClient("TeacherFindApi");

    public async Task<ApiRequestResult<T>> GetAsync<T>(string url)
    {
        try
        {
            var client = GetClient();
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                return new ApiRequestResult<T> { IsSuccess = true, Data = data, StatusCode = (int)response.StatusCode };
            }
            return new ApiRequestResult<T> { IsSuccess = false, Message = $"Hata: {response.StatusCode}", StatusCode = (int)response.StatusCode };
        }
        catch (Exception ex) { return new ApiRequestResult<T> { IsSuccess = false, Message = ex.Message, StatusCode = 500 }; }
    }

    public async Task<ApiRequestResult<T>> PostAsync<T, TData>(string url, TData data)
    {
        try
        {
            var client = GetClient();
            var response = await client.PostAsJsonAsync(url, data);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>();
                return new ApiRequestResult<T> { IsSuccess = true, Data = result, StatusCode = (int)response.StatusCode };
            }
            return new ApiRequestResult<T> { IsSuccess = false, Message = $"Hata: {response.StatusCode}", StatusCode = (int)response.StatusCode };
        }
        catch (Exception ex) { return new ApiRequestResult<T> { IsSuccess = false, Message = ex.Message, StatusCode = 500 }; }
    }

    public async Task<ApiRequestResult<T>> PutAsync<T, TData>(string url, TData data)
    {
        try
        {
            var client = GetClient();
            var response = await client.PutAsJsonAsync(url, data);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<T>();
                return new ApiRequestResult<T> { IsSuccess = true, Data = result, StatusCode = (int)response.StatusCode };
            }
            return new ApiRequestResult<T> { IsSuccess = false, Message = $"Hata: {response.StatusCode}", StatusCode = (int)response.StatusCode };
        }
        catch (Exception ex) { return new ApiRequestResult<T> { IsSuccess = false, Message = ex.Message, StatusCode = 500 }; }
    }

    public async Task<ApiRequestResult<bool>> DeleteAsync(string url)
    {
        try
        {
            var client = GetClient();
            var response = await client.DeleteAsync(url);
            return new ApiRequestResult<bool> { IsSuccess = response.IsSuccessStatusCode, Data = response.IsSuccessStatusCode, StatusCode = (int)response.StatusCode };
        }
        catch (Exception ex) { return new ApiRequestResult<bool> { IsSuccess = false, Message = ex.Message, Data = false, StatusCode = 500 }; }
    }
}