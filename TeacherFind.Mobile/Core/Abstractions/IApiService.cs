using Microsoft.Maui.Storage;

namespace TeacherFind.Mobile.Core.Abstractions
{
    public interface IApiService
    {
        Task<T> GetAsync<T>(string endpoint);

        Task<bool> PutAsync<TRequest>(string endpoint, TRequest request);

        Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest request);

        Task<TResponse> UploadFileAsync<TResponse>(
            string endpoint,
            FileResult file,
            string formFieldName = "file");

        string ToAbsoluteUrl(string? relativeOrAbsoluteUrl);
    }
}
