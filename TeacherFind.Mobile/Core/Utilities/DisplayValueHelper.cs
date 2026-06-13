using System.Net;
using System.Text.RegularExpressions;
using TeacherFind.Contracts.Tutors;
using TeacherFind.Mobile.Core.Abstractions;

namespace TeacherFind.Mobile.Core.Utilities;

public static partial class DisplayValueHelper
{
    public static string ToPlainText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var withoutTags = HtmlTagRegex().Replace(value, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);

        return WhitespaceRegex()
            .Replace(decoded, " ")
            .Trim();
    }

    public static string TruncatePlainText(string? value, int maxLength)
    {
        var plainText = ToPlainText(value);

        if (maxLength <= 0 || plainText.Length <= maxLength)
            return plainText;

        return plainText[..maxLength].TrimEnd() + "...";
    }

    public static string ResolveTutorImageUrl(
        IApiService apiService,
        IEnumerable<ListingPhotoDto>? photos,
        params string?[] fallbackUrls)
    {
        var imageUrl = photos?
            .OrderByDescending(x => x.IsMain)
            .ThenBy(x => x.SortOrder)
            .Select(x => x.PhotoUrl)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        imageUrl ??= fallbackUrls.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

        return apiService.ToAbsoluteUrl(imageUrl);
    }

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("\\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();
}
