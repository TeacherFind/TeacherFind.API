using System.Threading.Tasks;
using TeacherFind.Mobile.Core.Models;

namespace TeacherFind.Mobile.Core.Abstractions;

public interface IApiService
{
    Task<ApiRequestResult<T>> GetAsync<T>(string url);
    Task<ApiRequestResult<T>> PostAsync<T, TData>(string url, TData data);
    Task<ApiRequestResult<T>> PutAsync<T, TData>(string url, TData data);
    Task<ApiRequestResult<bool>> DeleteAsync(string url);
}