using Microsoft.Extensions.DependencyInjection;
using TeacherFind.Contracts.Auth;
using TeacherFind.Domain.Enums;
using TeacherFind.Mobile.Core.Abstractions;
using TeacherFind.Mobile.Features.Profile.Views;

namespace TeacherFind.Mobile.Core.Navigation;

internal static class ProfileNavigationHelper
{
    public static async Task<Page?> CreateCurrentUserProfilePageAsync(IServiceProvider? services)
    {
        if (services is null)
            return null;

        var apiService = services.GetService<IApiService>();
        if (apiService is null)
            return null;

        var currentUser = await apiService.GetAsync<MeResponse>("api/auth/me");
        if (currentUser is null)
            return null;

        var pageType = ResolveProfilePageType(currentUser);
        if (pageType is null)
            return null;

        return services.GetService(pageType) as Page
            ?? ActivatorUtilities.CreateInstance(services, pageType) as Page;
    }

    private static Type? ResolveProfilePageType(MeResponse currentUser)
    {
        if (IsRole(currentUser, UserRole.Tutor))
            return typeof(TutorProfilePage);

        if (IsRole(currentUser, UserRole.Student))
            return typeof(StudentProfilePage);

        return null;
    }

    private static bool IsRole(MeResponse currentUser, UserRole role)
        => currentUser.RoleValue == (int)role
            || string.Equals(currentUser.Role, role.ToString(), StringComparison.OrdinalIgnoreCase);
}
