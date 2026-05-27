using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Mobile.Core.Abstractions;

namespace TeacherFind.Mobile.Core.Storage;

public class TokenStorageService : ITokenStorage
{
    private const string AuthTokenKey = "auth_token";

    public async Task SaveTokenAsync(string token)
    {
        await SecureStorage.Default.SetAsync(AuthTokenKey, token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(AuthTokenKey);
    }

    public async Task ClearTokenAsync()
    {
        SecureStorage.Default.Remove(AuthTokenKey);
        await Task.CompletedTask;
    }
}
