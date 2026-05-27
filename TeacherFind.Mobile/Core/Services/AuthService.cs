using System;
using System.Threading.Tasks;
using TeacherFind.Contracts.Auth;
using TeacherFind.Mobile.Core.Abstractions;
using TeacherFind.Mobile.Core.Models;

namespace TeacherFind.Mobile.Core.Services;

public class AuthService : IAuthService
{
    private const string LoginApiPath = "api/auth/login";
    private readonly IApiService _apiService;
    private readonly ITokenStorage _tokenStorage;

    public AuthService(IApiService apiService, ITokenStorage tokenStorage)
    {
        _apiService = apiService;
        _tokenStorage = tokenStorage;
    }

    public async Task<ApiRequestResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        // _apiService.PostAsync zaten ApiRequestResult<LoginResponse> döndürüyor.
        // Doğrudan gelen sonucu return ediyoruz, ekstra dönüşüm yapmıyoruz.
        return await _apiService.PostAsync<LoginResponse, LoginRequest>(LoginApiPath, request);
    }
}