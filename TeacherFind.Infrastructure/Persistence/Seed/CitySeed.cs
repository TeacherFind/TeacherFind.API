using Microsoft.EntityFrameworkCore;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Persistence.Seed;

public static class CitySeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Cities.AnyAsync())
            return;

        var cities = new List<City>
        {
            new City { PlateCode = 1, Name = "ADANA", IsActive = true },
            new City { PlateCode = 2, Name = "ADIYAMAN", IsActive = true },
            new City { PlateCode = 3, Name = "AFYONKARAHİSAR", IsActive = true },
            new City { PlateCode = 4, Name = "AĞRI", IsActive = true },
            new City { PlateCode = 5, Name = "AMASYA", IsActive = true },
            new City { PlateCode = 6, Name = "ANKARA", IsActive = true },
            new City { PlateCode = 7, Name = "ANTALYA", IsActive = true },
            new City { PlateCode = 8, Name = "ARTVİN", IsActive = true },
            new City { PlateCode = 9, Name = "AYDIN", IsActive = true },
            new City { PlateCode = 10, Name = "BALIKESİR", IsActive = true },
            new City { PlateCode = 11, Name = "BİLECİK", IsActive = true },
            new City { PlateCode = 12, Name = "BİNGÖL", IsActive = true },
            new City { PlateCode = 13, Name = "BİTLİS", IsActive = true },
            new City { PlateCode = 14, Name = "BOLU", IsActive = true },
            new City { PlateCode = 15, Name = "BURDUR", IsActive = true },
            new City { PlateCode = 16, Name = "BURSA", IsActive = true },
            new City { PlateCode = 17, Name = "ÇANAKKALE", IsActive = true },
            new City { PlateCode = 18, Name = "ÇANKIRI", IsActive = true },
            new City { PlateCode = 19, Name = "ÇORUM", IsActive = true },
            new City { PlateCode = 20, Name = "DENİZLİ", IsActive = true },
            new City { PlateCode = 21, Name = "DİYARBAKIR", IsActive = true },
            new City { PlateCode = 22, Name = "EDİRNE", IsActive = true },
            new City { PlateCode = 23, Name = "ELAZIĞ", IsActive = true },
            new City { PlateCode = 24, Name = "ERZİNCAN", IsActive = true },
            new City { PlateCode = 25, Name = "ERZURUM", IsActive = true },
            new City { PlateCode = 26, Name = "ESKİŞEHİR", IsActive = true },
            new City { PlateCode = 27, Name = "GAZİANTEP", IsActive = true },
            new City { PlateCode = 28, Name = "GİRESUN", IsActive = true },
            new City { PlateCode = 29, Name = "GÜMÜŞHANE", IsActive = true },
            new City { PlateCode = 30, Name = "HAKKARİ", IsActive = true },
            new City { PlateCode = 31, Name = "HATAY", IsActive = true },
            new City { PlateCode = 32, Name = "ISPARTA", IsActive = true },
            new City { PlateCode = 33, Name = "MERSİN", IsActive = true },
            new City { PlateCode = 34, Name = "İSTANBUL", IsActive = true },
            new City { PlateCode = 35, Name = "İZMİR", IsActive = true },
            new City { PlateCode = 36, Name = "KARS", IsActive = true },
            new City { PlateCode = 37, Name = "KASTAMONU", IsActive = true },
            new City { PlateCode = 38, Name = "KAYSERİ", IsActive = true },
            new City { PlateCode = 39, Name = "KIRKLARELİ", IsActive = true },
            new City { PlateCode = 40, Name = "KIRŞEHİR", IsActive = true },
            new City { PlateCode = 41, Name = "KOCAELİ", IsActive = true },
            new City { PlateCode = 42, Name = "KONYA", IsActive = true },
            new City { PlateCode = 43, Name = "KÜTAHYA", IsActive = true },
            new City { PlateCode = 44, Name = "MALATYA", IsActive = true },
            new City { PlateCode = 45, Name = "MANİSA", IsActive = true },
            new City { PlateCode = 46, Name = "KAHRAMANMARAŞ", IsActive = true },
            new City { PlateCode = 47, Name = "MARDİN", IsActive = true },
            new City { PlateCode = 48, Name = "MUĞLA", IsActive = true },
            new City { PlateCode = 49, Name = "MUŞ", IsActive = true },
            new City { PlateCode = 50, Name = "NEVŞEHİR", IsActive = true },
            new City { PlateCode = 51, Name = "NİĞDE", IsActive = true },
            new City { PlateCode = 52, Name = "ORDU", IsActive = true },
            new City { PlateCode = 53, Name = "RİZE", IsActive = true },
            new City { PlateCode = 54, Name = "SAKARYA", IsActive = true },
            new City { PlateCode = 55, Name = "SAMSUN", IsActive = true },
            new City { PlateCode = 56, Name = "SİİRT", IsActive = true },
            new City { PlateCode = 57, Name = "SİNOP", IsActive = true },
            new City { PlateCode = 58, Name = "SİVAS", IsActive = true },
            new City { PlateCode = 59, Name = "TEKİRDAĞ", IsActive = true },
            new City { PlateCode = 60, Name = "TOKAT", IsActive = true },
            new City { PlateCode = 61, Name = "TRABZON", IsActive = true },
            new City { PlateCode = 62, Name = "TUNCELİ", IsActive = true },
            new City { PlateCode = 63, Name = "ŞANLIURFA", IsActive = true },
            new City { PlateCode = 64, Name = "UŞAK", IsActive = true },
            new City { PlateCode = 65, Name = "VAN", IsActive = true },
            new City { PlateCode = 66, Name = "YOZGAT", IsActive = true },
            new City { PlateCode = 67, Name = "ZONGULDAK", IsActive = true },
            new City { PlateCode = 68, Name = "AKSARAY", IsActive = true },
            new City { PlateCode = 69, Name = "BAYBURT", IsActive = true },
            new City { PlateCode = 70, Name = "KARAMAN", IsActive = true },
            new City { PlateCode = 71, Name = "KIRIKKALE", IsActive = true },
            new City { PlateCode = 72, Name = "BATMAN", IsActive = true },
            new City { PlateCode = 73, Name = "ŞIRNAK", IsActive = true },
            new City { PlateCode = 74, Name = "BARTIN", IsActive = true },
            new City { PlateCode = 75, Name = "ARDAHAN", IsActive = true },
            new City { PlateCode = 76, Name = "IĞDIR", IsActive = true },
            new City { PlateCode = 77, Name = "YALOVA", IsActive = true },
            new City { PlateCode = 78, Name = "KARABÜK", IsActive = true },
            new City { PlateCode = 79, Name = "KİLİS", IsActive = true },
            new City { PlateCode = 80, Name = "OSMANİYE", IsActive = true },
            new City { PlateCode = 81, Name = "DÜZCE", IsActive = true },
        };

        await context.Cities.AddRangeAsync(cities);
        await context.SaveChangesAsync();
    }
}