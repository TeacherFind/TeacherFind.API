using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TeacherFind.Infrastructure.Notifications;

public static class FirebaseAdminInitializer
{
    private const string CredentialJsonEnvironmentVariable = "FIREBASE_CREDENTIAL_JSON";
    private const string CredentialPathEnvironmentVariable = "FIREBASE_CREDENTIAL_PATH";

    public static bool Initialize(IConfiguration configuration, ILogger logger, string contentRootPath)
    {
        if (FirebaseApp.DefaultInstance != null)
        {
            logger.LogInformation("Firebase Admin SDK zaten başlatılmış.");
            return true;
        }

        try
        {
            var projectId = configuration["Firebase:ProjectId"];
            var credentialJson = Environment.GetEnvironmentVariable(CredentialJsonEnvironmentVariable);

            if (!string.IsNullOrWhiteSpace(credentialJson))
            {
                CreateApp(CreateCredentialFromJson(credentialJson), projectId);
                logger.LogInformation("Firebase Admin SDK FIREBASE_CREDENTIAL_JSON ile başarıyla başlatıldı.");
                return true;
            }

            var credentialPath = Environment.GetEnvironmentVariable(CredentialPathEnvironmentVariable)
                ?? configuration["Firebase:CredentialPath"]
                ?? configuration["Firebase:ServiceAccountPath"];

            if (!string.IsNullOrWhiteSpace(credentialPath))
            {
                var resolvedPath = ResolvePath(credentialPath, contentRootPath);

                if (File.Exists(resolvedPath))
                {
                    var pathCredentialJson = File.ReadAllText(resolvedPath);
                    CreateApp(CreateCredentialFromJson(pathCredentialJson), projectId);
                    logger.LogInformation("Firebase Admin SDK credential path ile başarıyla başlatıldı. Path: {CredentialPath}", resolvedPath);
                    return true;
                }

                logger.LogWarning("Firebase credential path bulundu ama dosya okunamadı. Path: {CredentialPath}", resolvedPath);
            }

            logger.LogWarning("Firebase credential bulunamadı. Firebase Admin SDK başlatılmadı. Push notification devre dışı.");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Firebase Admin SDK başlatılırken hata oluştu. Push notification devre dışı.");
            return false;
        }
    }

    private static void CreateApp(GoogleCredential credential, string? projectId)
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = credential,
            ProjectId = string.IsNullOrWhiteSpace(projectId) ? null : projectId.Trim()
        });
    }

    private static GoogleCredential CreateCredentialFromJson(string credentialJson)
        => CredentialFactory
            .FromJson<ServiceAccountCredential>(credentialJson)
            .ToGoogleCredential();

    private static string ResolvePath(string credentialPath, string contentRootPath)
    {
        var trimmedPath = credentialPath.Trim();

        return Path.IsPathRooted(trimmedPath)
            ? trimmedPath
            : Path.GetFullPath(Path.Combine(contentRootPath, trimmedPath));
    }
}
