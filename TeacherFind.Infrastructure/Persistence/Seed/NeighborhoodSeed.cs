using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Infrastructure.Persistence.Seed;

public static class NeighborhoodSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Console.OutputEncoding = Encoding.UTF8;

        if (await context.Neighborhoods.AnyAsync())
        {
            Console.WriteLine("Mahalle verisi zaten mevcut. Seed atlandı.");
            return;
        }

        var filePath = FindSeedFile();

        if (filePath is null)
        {
            Console.WriteLine("turkiye-il-ilce-mahalle.json dosyası bulunamadı.");
            return;
        }

        var json = await ReadJsonWithBestEncodingAsync(filePath);

        using var document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            Console.WriteLine("Mahalle JSON formatı beklenen object formatında değil.");
            return;
        }

        var districts = await context.Districts
            .Include(x => x.City)
            .AsNoTracking()
            .ToListAsync();

        var districtMap = districts
            .GroupBy(x => CreateDistrictKey(x.City.Name, x.Name))
            .ToDictionary(x => x.Key, x => x.First());

        var neighborhoods = new List<Neighborhood>();
        var unmatchedDistricts = new List<string>();

        var nextCode = await GetNextNeighborhoodCodeAsync(context);

        foreach (var cityProperty in document.RootElement.EnumerateObject())
        {
            var cityName = FixMojibake(cityProperty.Name);

            if (cityProperty.Value.ValueKind != JsonValueKind.Object)
                continue;

            foreach (var districtProperty in cityProperty.Value.EnumerateObject())
            {
                var districtName = FixMojibake(districtProperty.Name);

                if (districtProperty.Value.ValueKind != JsonValueKind.Array)
                    continue;

                var key = CreateDistrictKey(cityName, districtName);

                if (!districtMap.TryGetValue(key, out var district))
                {
                    unmatchedDistricts.Add($"{cityName} / {districtName}");
                    continue;
                }

                foreach (var neighborhoodElement in districtProperty.Value.EnumerateArray())
                {
                    if (neighborhoodElement.ValueKind != JsonValueKind.String)
                        continue;

                    var neighborhoodName = neighborhoodElement.GetString();

                    if (string.IsNullOrWhiteSpace(neighborhoodName))
                        continue;

                    neighborhoodName = NormalizeNeighborhoodName(FixMojibake(neighborhoodName));

                    var alreadyAdded = neighborhoods.Any(x =>
                        x.DistrictId == district.Id &&
                        NormalizeKey(x.Name) == NormalizeKey(neighborhoodName));

                    if (alreadyAdded)
                        continue;

                    neighborhoods.Add(new Neighborhood
                    {
                        Id = Guid.NewGuid(),
                        Code = nextCode++,
                        Name = neighborhoodName,
                        DistrictId = district.Id,
                        IsActive = true
                    });
                }
            }
        }

        if (unmatchedDistricts.Count > 0)
        {
            Console.WriteLine($"Eşleşmeyen ilçe sayısı: {unmatchedDistricts.Count}");

            foreach (var item in unmatchedDistricts.Take(50))
                Console.WriteLine($"İlçe eşleşmedi: {item}");

            if (unmatchedDistricts.Count > 50)
                Console.WriteLine($"... {unmatchedDistricts.Count - 50} ilçe daha eşleşmedi.");
        }

        if (neighborhoods.Count == 0)
        {
            Console.WriteLine("Eklenecek mahalle bulunamadı. İl/ilçe isim eşleşmeleri veya JSON encoding kontrol edilmeli.");
            return;
        }

        await context.Neighborhoods.AddRangeAsync(neighborhoods);
        await context.SaveChangesAsync();

        Console.WriteLine($"{neighborhoods.Count} mahalle/köy başarıyla eklendi.");
    }

    private static async Task<int> GetNextNeighborhoodCodeAsync(AppDbContext context)
    {
        if (!await context.Neighborhoods.AnyAsync())
            return 1_000_000;

        return await context.Neighborhoods.MaxAsync(x => x.Code) + 1;
    }

    private static string? FindSeedFile()
    {
        var possiblePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Persistence", "Seed", "Data", "turkiye-il-ilce-mahalle.json"),
            Path.Combine(AppContext.BaseDirectory, "Seed", "Data", "turkiye-il-ilce-mahalle.json"),
            Path.Combine(AppContext.BaseDirectory, "Data", "turkiye-il-ilce-mahalle.json"),

            Path.Combine(Directory.GetCurrentDirectory(), "Persistence", "Seed", "Data", "turkiye-il-ilce-mahalle.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "Seed", "Data", "turkiye-il-ilce-mahalle.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "Data", "turkiye-il-ilce-mahalle.json"),

            Path.Combine(Directory.GetCurrentDirectory(), "TeacherFind.Infrastructure", "Persistence", "Seed", "Data", "turkiye-il-ilce-mahalle.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "TeacherFind.Infrastructure", "Persistence", "Seed", "Data", "turkiye-il-ilce-mahalle.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "src", "TeacherFind.Infrastructure", "Persistence", "Seed", "Data", "turkiye-il-ilce-mahalle.json")
        };

        return possiblePaths.FirstOrDefault(File.Exists);
    }

    private static async Task<string> ReadJsonWithBestEncodingAsync(string filePath)
    {
        var bytes = await File.ReadAllBytesAsync(filePath);

        string text;

        if (bytes.Length >= 3 &&
            bytes[0] == 0xEF &&
            bytes[1] == 0xBB &&
            bytes[2] == 0xBF)
        {
            text = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        }
        else
        {
            var utf8Text = Encoding.UTF8.GetString(bytes);

            if (!LooksLikeMojibake(utf8Text))
            {
                text = utf8Text;
            }
            else
            {
                var windows1254 = Encoding.GetEncoding("windows-1254");
                var windows1254Text = windows1254.GetString(bytes);

                text = !LooksLikeMojibake(windows1254Text)
                    ? windows1254Text
                    : FixMojibake(utf8Text);
            }
        }

        return text
            .TrimStart('\uFEFF')
            .Trim();
    }

    private static bool LooksLikeMojibake(string value)
    {
        return value.Contains('Ã') ||
               value.Contains('Ä') ||
               value.Contains('Å') ||
               value.Contains('�');
    }

    private static string FixMojibake(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var fixedValue = value
            .Replace("Ä°", "İ")
            .Replace("Ä±", "ı")
            .Replace("Ã‡", "Ç")
            .Replace("Ã§", "ç")
            .Replace("Ã–", "Ö")
            .Replace("Ã¶", "ö")
            .Replace("Ãœ", "Ü")
            .Replace("Ã¼", "ü")
            .Replace("Äž", "Ğ")
            .Replace("ÄŸ", "ğ")
            .Replace("Åž", "Ş")
            .Replace("ÅŸ", "ş")
            .Replace("Å", "Ş")
            .Replace("Å", "ş")
            .Replace("Ä", "Ğ")
            .Replace("Ä", "ğ")
            .Replace("Ä°", "İ")
            .Replace("Ä", "İ")
            .Replace("Å", "Ş");

        return fixedValue;
    }

    private static string CreateDistrictKey(string cityName, string districtName)
    {
        return $"{NormalizeKey(cityName)}::{NormalizeKey(districtName)}";
    }

    private static string NormalizeKey(string value)
    {
        value = FixMojibake(value);

        return value
            .Trim()
            .Replace("İ", "I")
            .Replace("İ", "I")
            .Replace("ı", "I")
            .Replace("Â", "A")
            .Replace("â", "A")
            .Replace("Ç", "C")
            .Replace("ç", "C")
            .Replace("Ğ", "G")
            .Replace("ğ", "G")
            .Replace("Ö", "O")
            .Replace("ö", "O")
            .Replace("Ş", "S")
            .Replace("ş", "S")
            .Replace("Ü", "U")
            .Replace("ü", "U")
            .Replace(".", "")
            .Replace("-", "")
            .Replace(" ", "")
            .ToUpperInvariant();
    }

    private static string NormalizeNeighborhoodName(string value)
    {
        return FixMojibake(value).Trim();
    }
}