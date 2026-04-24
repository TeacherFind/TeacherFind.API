using Microsoft.EntityFrameworkCore;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Seeds;

/// <summary>
/// Güncel temiz üniversite seed dosyası.
///
/// Bu dosya eski SQL listesindeki kapanmış / adı değişmiş / meslek yüksekokulu kayıtlarını temizler
/// ve Türkiye'deki aktif üniversite listesini yeniden oluşturur.
///
/// Not:
/// - Code alanı sistem içi sabit seed kodudur.
/// - CityPlateCode il plaka kodudur.
/// - IsActive tüm güncel kayıtlar için true verilmiştir.
/// - Canlı veritabanında TeacherProfile veya Department bağlantısı varsa silme işlemi FK hatası verebilir.
///   Geliştirme ortamında temiz seed için tasarlanmıştır.
/// </summary>
public static class UniversitySeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var universities = GetUniversities();
        var currentNames = universities.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Eski listede olup güncel listede olmayanları siler.
        // Örnek: İSTANBUL ŞEHİR ÜNİVERSİTESİ, İSTANBUL AYVANSARAY ÜNİVERSİTESİ,
        // ADANA BİLİM VE TEKNOLOJİ ÜNİVERSİTESİ, OKAN ÜNİVERSİTESİ, NİŞANTAŞI ÜNİVERSİTESİ vb.
        var obsoleteUniversities = await context.Universities
            .Where(x => !currentNames.Contains(x.Name))
            .ToListAsync();

        if (obsoleteUniversities.Count > 0)
        {
            context.Universities.RemoveRange(obsoleteUniversities);
        }

        foreach (var seedUniversity in universities)
        {
            var existingUniversity = await context.Universities
                .FirstOrDefaultAsync(x => x.Name == seedUniversity.Name || x.Code == seedUniversity.Code);

            if (existingUniversity is null)
            {
                await context.Universities.AddAsync(seedUniversity);
                continue;
            }

            existingUniversity.Code = seedUniversity.Code;
            existingUniversity.Name = seedUniversity.Name;
            existingUniversity.CityPlateCode = seedUniversity.CityPlateCode;
            existingUniversity.IsActive = true;
        }

        await context.SaveChangesAsync();
    }

    private static List<University> GetUniversities()
    {
        return new List<University>
        {
            new University { Code = 1000, Name = "ABDULLAH GÜL ÜNİVERSİTESİ", CityPlateCode = 38, IsActive = true },
            new University { Code = 1001, Name = "ACIBADEM MEHMET ALİ AYDINLAR ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1002, Name = "ADANA ALPARSLAN TÜRKEŞ BİLİM VE TEKNOLOJİ ÜNİVERSİTESİ", CityPlateCode = 1, IsActive = true },
            new University { Code = 1003, Name = "ADIYAMAN ÜNİVERSİTESİ", CityPlateCode = 2, IsActive = true },
            new University { Code = 1004, Name = "AFYON KOCATEPE ÜNİVERSİTESİ", CityPlateCode = 3, IsActive = true },
            new University { Code = 1005, Name = "AFYONKARAHİSAR SAĞLIK BİLİMLERİ ÜNİVERSİTESİ", CityPlateCode = 3, IsActive = true },
            new University { Code = 1006, Name = "AKDENİZ ÜNİVERSİTESİ", CityPlateCode = 7, IsActive = true },
            new University { Code = 1007, Name = "AKSARAY ÜNİVERSİTESİ", CityPlateCode = 68, IsActive = true },
            new University { Code = 1008, Name = "ALANYA ALAADDİN KEYKUBAT ÜNİVERSİTESİ", CityPlateCode = 7, IsActive = true },
            new University { Code = 1009, Name = "ALANYA ÜNİVERSİTESİ", CityPlateCode = 7, IsActive = true },
            new University { Code = 1010, Name = "ALTINBAŞ ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1011, Name = "AMASYA ÜNİVERSİTESİ", CityPlateCode = 5, IsActive = true },
            new University { Code = 1012, Name = "ANADOLU ÜNİVERSİTESİ", CityPlateCode = 26, IsActive = true },
            new University { Code = 1013, Name = "ANKARA BİLİM ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1014, Name = "ANKARA HACI BAYRAM VELİ ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1015, Name = "ANKARA MEDİPOL ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1016, Name = "ANKARA MÜZİK VE GÜZEL SANATLAR ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1017, Name = "ANKARA SOSYAL BİLİMLER ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1018, Name = "ANKARA YILDIRIM BEYAZIT ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1019, Name = "ANKARA ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1020, Name = "ANTALYA BELEK ÜNİVERSİTESİ", CityPlateCode = 7, IsActive = true },
            new University { Code = 1021, Name = "ANTALYA BİLİM ÜNİVERSİTESİ", CityPlateCode = 7, IsActive = true },
            new University { Code = 1022, Name = "ARDAHAN ÜNİVERSİTESİ", CityPlateCode = 75, IsActive = true },
            new University { Code = 1023, Name = "ARTVİN ÇORUH ÜNİVERSİTESİ", CityPlateCode = 8, IsActive = true },
            new University { Code = 1024, Name = "ATATÜRK ÜNİVERSİTESİ", CityPlateCode = 25, IsActive = true },
            new University { Code = 1025, Name = "ATILIM ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1026, Name = "AVRASYA ÜNİVERSİTESİ", CityPlateCode = 61, IsActive = true },
            new University { Code = 1027, Name = "AYDIN ADNAN MENDERES ÜNİVERSİTESİ", CityPlateCode = 9, IsActive = true },
            new University { Code = 1028, Name = "AĞRI İBRAHİM ÇEÇEN ÜNİVERSİTESİ", CityPlateCode = 4, IsActive = true },
            new University { Code = 1029, Name = "BAHÇEŞEHİR ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1030, Name = "BALIKESİR ÜNİVERSİTESİ", CityPlateCode = 10, IsActive = true },
            new University { Code = 1031, Name = "BANDIRMA ONYEDİ EYLÜL ÜNİVERSİTESİ", CityPlateCode = 10, IsActive = true },
            new University { Code = 1032, Name = "BARTIN ÜNİVERSİTESİ", CityPlateCode = 74, IsActive = true },
            new University { Code = 1033, Name = "BATMAN ÜNİVERSİTESİ", CityPlateCode = 72, IsActive = true },
            new University { Code = 1034, Name = "BAYBURT ÜNİVERSİTESİ", CityPlateCode = 69, IsActive = true },
            new University { Code = 1035, Name = "BAŞKENT ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1036, Name = "BEYKOZ ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1037, Name = "BEZM-İ ÂLEM VAKIF ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1038, Name = "BOLU ABANT İZZET BAYSAL ÜNİVERSİTESİ", CityPlateCode = 14, IsActive = true },
            new University { Code = 1039, Name = "BOĞAZİÇİ ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1040, Name = "BURDUR MEHMET AKİF ERSOY ÜNİVERSİTESİ", CityPlateCode = 15, IsActive = true },
            new University { Code = 1041, Name = "BURSA TEKNİK ÜNİVERSİTESİ", CityPlateCode = 16, IsActive = true },
            new University { Code = 1042, Name = "BURSA ULUDAĞ ÜNİVERSİTESİ", CityPlateCode = 16, IsActive = true },
            new University { Code = 1043, Name = "BİLECİK ŞEYH EDEBALİ ÜNİVERSİTESİ", CityPlateCode = 11, IsActive = true },
            new University { Code = 1044, Name = "BİNGÖL ÜNİVERSİTESİ", CityPlateCode = 12, IsActive = true },
            new University { Code = 1045, Name = "BİRUNİ ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1046, Name = "BİTLİS EREN ÜNİVERSİTESİ", CityPlateCode = 13, IsActive = true },
            new University { Code = 1047, Name = "DEMİROĞLU BİLİM ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1048, Name = "DOKUZ EYLÜL ÜNİVERSİTESİ", CityPlateCode = 35, IsActive = true },
            new University { Code = 1049, Name = "DOĞUŞ ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1050, Name = "DÜZCE ÜNİVERSİTESİ", CityPlateCode = 81, IsActive = true },
            new University { Code = 1051, Name = "DİCLE ÜNİVERSİTESİ", CityPlateCode = 21, IsActive = true },
            new University { Code = 1052, Name = "EGE ÜNİVERSİTESİ", CityPlateCode = 35, IsActive = true },
            new University { Code = 1053, Name = "ERCİYES ÜNİVERSİTESİ", CityPlateCode = 38, IsActive = true },
            new University { Code = 1054, Name = "ERZURUM TEKNİK ÜNİVERSİTESİ", CityPlateCode = 25, IsActive = true },
            new University { Code = 1055, Name = "ERZİNCAN BİNALİ YILDIRIM ÜNİVERSİTESİ", CityPlateCode = 24, IsActive = true },
            new University { Code = 1056, Name = "ESKİŞEHİR OSMANGAZİ ÜNİVERSİTESİ", CityPlateCode = 26, IsActive = true },
            new University { Code = 1057, Name = "ESKİŞEHİR TEKNİK ÜNİVERSİTESİ", CityPlateCode = 26, IsActive = true },
            new University { Code = 1058, Name = "FATİH SULTAN MEHMET VAKIF ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1059, Name = "FENERBAHÇE ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1060, Name = "FIRAT ÜNİVERSİTESİ", CityPlateCode = 23, IsActive = true },
            new University { Code = 1061, Name = "GALATASARAY ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1062, Name = "GAZİ ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1063, Name = "GAZİANTEP ÜNİVERSİTESİ", CityPlateCode = 27, IsActive = true },
            new University { Code = 1064, Name = "GAZİANTEP İSLAM BİLİM VE TEKNOLOJİ ÜNİVERSİTESİ", CityPlateCode = 27, IsActive = true },
            new University { Code = 1065, Name = "GEBZE TEKNİK ÜNİVERSİTESİ", CityPlateCode = 41, IsActive = true },
            new University { Code = 1066, Name = "GÜMÜŞHANE ÜNİVERSİTESİ", CityPlateCode = 29, IsActive = true },
            new University { Code = 1067, Name = "GİRESUN ÜNİVERSİTESİ", CityPlateCode = 28, IsActive = true },
            new University { Code = 1068, Name = "HACETTEPE ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1069, Name = "HAKKARİ ÜNİVERSİTESİ", CityPlateCode = 30, IsActive = true },
            new University { Code = 1070, Name = "HALİÇ ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1071, Name = "HARRAN ÜNİVERSİTESİ", CityPlateCode = 63, IsActive = true },
            new University { Code = 1072, Name = "HASAN KALYONCU ÜNİVERSİTESİ", CityPlateCode = 27, IsActive = true },
            new University { Code = 1073, Name = "HATAY MUSTAFA KEMAL ÜNİVERSİTESİ", CityPlateCode = 31, IsActive = true },
            new University { Code = 1074, Name = "HİTİT ÜNİVERSİTESİ", CityPlateCode = 19, IsActive = true },
            new University { Code = 1075, Name = "ISPARTA UYGULAMALI BİLİMLER ÜNİVERSİTESİ", CityPlateCode = 32, IsActive = true },
            new University { Code = 1076, Name = "IĞDIR ÜNİVERSİTESİ", CityPlateCode = 76, IsActive = true },
            new University { Code = 1077, Name = "KADİR HAS ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1078, Name = "KAFKAS ÜNİVERSİTESİ", CityPlateCode = 36, IsActive = true },
            new University { Code = 1079, Name = "KAHRAMANMARAŞ SÜTÇÜ İMAM ÜNİVERSİTESİ", CityPlateCode = 46, IsActive = true },
            new University { Code = 1080, Name = "KAHRAMANMARAŞ İSTİKLAL ÜNİVERSİTESİ", CityPlateCode = 46, IsActive = true },
            new University { Code = 1081, Name = "KAPADOKYA ÜNİVERSİTESİ", CityPlateCode = 50, IsActive = true },
            new University { Code = 1082, Name = "KARABÜK ÜNİVERSİTESİ", CityPlateCode = 78, IsActive = true },
            new University { Code = 1083, Name = "KARADENİZ TEKNİK ÜNİVERSİTESİ", CityPlateCode = 61, IsActive = true },
            new University { Code = 1084, Name = "KARAMANOĞLU MEHMETBEY ÜNİVERSİTESİ", CityPlateCode = 70, IsActive = true },
            new University { Code = 1085, Name = "KASTAMONU ÜNİVERSİTESİ", CityPlateCode = 37, IsActive = true },
            new University { Code = 1086, Name = "KAYSERİ ÜNİVERSİTESİ", CityPlateCode = 38, IsActive = true },
            new University { Code = 1087, Name = "KIRIKKALE ÜNİVERSİTESİ", CityPlateCode = 71, IsActive = true },
            new University { Code = 1088, Name = "KIRKLARELİ ÜNİVERSİTESİ", CityPlateCode = 39, IsActive = true },
            new University { Code = 1089, Name = "KIRŞEHİR AHİ EVRAN ÜNİVERSİTESİ", CityPlateCode = 40, IsActive = true },
            new University { Code = 1090, Name = "KOCAELİ SAĞLIK VE TEKNOLOJİ ÜNİVERSİTESİ", CityPlateCode = 41, IsActive = true },
            new University { Code = 1091, Name = "KOCAELİ ÜNİVERSİTESİ", CityPlateCode = 41, IsActive = true },
            new University { Code = 1092, Name = "KONYA GIDA VE TARIM ÜNİVERSİTESİ", CityPlateCode = 42, IsActive = true },
            new University { Code = 1093, Name = "KONYA TEKNİK ÜNİVERSİTESİ", CityPlateCode = 42, IsActive = true },
            new University { Code = 1094, Name = "KOÇ ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1095, Name = "KTO KARATAY ÜNİVERSİTESİ", CityPlateCode = 42, IsActive = true },
            new University { Code = 1096, Name = "KÜTAHYA DUMLUPINAR ÜNİVERSİTESİ", CityPlateCode = 43, IsActive = true },
            new University { Code = 1097, Name = "KÜTAHYA SAĞLIK BİLİMLERİ ÜNİVERSİTESİ", CityPlateCode = 43, IsActive = true },
            new University { Code = 1098, Name = "KİLİS 7 ARALIK ÜNİVERSİTESİ", CityPlateCode = 79, IsActive = true },
            new University { Code = 1099, Name = "LOKMAN HEKİM ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1100, Name = "MALATYA TURGUT ÖZAL ÜNİVERSİTESİ", CityPlateCode = 44, IsActive = true },
            new University { Code = 1101, Name = "MALTEPE ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1102, Name = "MANİSA CELAL BAYAR ÜNİVERSİTESİ", CityPlateCode = 45, IsActive = true },
            new University { Code = 1103, Name = "MARDİN ARTUKLU ÜNİVERSİTESİ", CityPlateCode = 47, IsActive = true },
            new University { Code = 1104, Name = "MARMARA ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1105, Name = "MEF ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1106, Name = "MERSİN ÜNİVERSİTESİ", CityPlateCode = 33, IsActive = true },
            new University { Code = 1107, Name = "MUDANYA ÜNİVERSİTESİ", CityPlateCode = 16, IsActive = true },
            new University { Code = 1108, Name = "MUNZUR ÜNİVERSİTESİ", CityPlateCode = 62, IsActive = true },
            new University { Code = 1109, Name = "MUĞLA SITKI KOÇMAN ÜNİVERSİTESİ", CityPlateCode = 48, IsActive = true },
            new University { Code = 1110, Name = "MUŞ ALPARSLAN ÜNİVERSİTESİ", CityPlateCode = 49, IsActive = true },
            new University { Code = 1111, Name = "MİMAR SİNAN GÜZEL SANATLAR ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1112, Name = "NECMETTİN ERBAKAN ÜNİVERSİTESİ", CityPlateCode = 42, IsActive = true },
            new University { Code = 1113, Name = "NEVŞEHİR HACI BEKTAŞ VELİ ÜNİVERSİTESİ", CityPlateCode = 50, IsActive = true },
            new University { Code = 1114, Name = "NUH NACİ YAZGAN ÜNİVERSİTESİ", CityPlateCode = 38, IsActive = true },
            new University { Code = 1115, Name = "NİĞDE ÖMER HALİSDEMİR ÜNİVERSİTESİ", CityPlateCode = 51, IsActive = true },
            new University { Code = 1116, Name = "ONDOKUZ MAYIS ÜNİVERSİTESİ", CityPlateCode = 55, IsActive = true },
            new University { Code = 1117, Name = "ORDU ÜNİVERSİTESİ", CityPlateCode = 52, IsActive = true },
            new University { Code = 1118, Name = "ORTA DOĞU TEKNİK ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1119, Name = "OSMANİYE KORKUT ATA ÜNİVERSİTESİ", CityPlateCode = 80, IsActive = true },
            new University { Code = 1120, Name = "OSTİM TEKNİK ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1121, Name = "PAMUKKALE ÜNİVERSİTESİ", CityPlateCode = 20, IsActive = true },
            new University { Code = 1122, Name = "PİRİ REİS ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1123, Name = "RECEP TAYYİP ERDOĞAN ÜNİVERSİTESİ", CityPlateCode = 53, IsActive = true },
            new University { Code = 1124, Name = "SABANCI ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1125, Name = "SAKARYA UYGULAMALI BİLİMLER ÜNİVERSİTESİ", CityPlateCode = 54, IsActive = true },
            new University { Code = 1126, Name = "SAKARYA ÜNİVERSİTESİ", CityPlateCode = 54, IsActive = true },
            new University { Code = 1127, Name = "SAMSUN ÜNİVERSİTESİ", CityPlateCode = 55, IsActive = true },
            new University { Code = 1128, Name = "SANKO ÜNİVERSİTESİ", CityPlateCode = 27, IsActive = true },
            new University { Code = 1129, Name = "SAĞLIK BİLİMLERİ ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1130, Name = "SELÇUK ÜNİVERSİTESİ", CityPlateCode = 42, IsActive = true },
            new University { Code = 1131, Name = "SÜLEYMAN DEMİREL ÜNİVERSİTESİ", CityPlateCode = 32, IsActive = true },
            new University { Code = 1132, Name = "SİNOP ÜNİVERSİTESİ", CityPlateCode = 57, IsActive = true },
            new University { Code = 1133, Name = "SİVAS BİLİM VE TEKNOLOJİ ÜNİVERSİTESİ", CityPlateCode = 58, IsActive = true },
            new University { Code = 1134, Name = "SİVAS CUMHURİYET ÜNİVERSİTESİ", CityPlateCode = 58, IsActive = true },
            new University { Code = 1135, Name = "SİİRT ÜNİVERSİTESİ", CityPlateCode = 56, IsActive = true },
            new University { Code = 1136, Name = "TARSUS ÜNİVERSİTESİ", CityPlateCode = 33, IsActive = true },
            new University { Code = 1137, Name = "TED ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1138, Name = "TEKİRDAĞ NAMIK KEMAL ÜNİVERSİTESİ", CityPlateCode = 59, IsActive = true },
            new University { Code = 1139, Name = "TOBB EKONOMİ VE TEKNOLOJİ ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1140, Name = "TOKAT GAZİOSMANPAŞA ÜNİVERSİTESİ", CityPlateCode = 60, IsActive = true },
            new University { Code = 1141, Name = "TOROS ÜNİVERSİTESİ", CityPlateCode = 33, IsActive = true },
            new University { Code = 1142, Name = "TRABZON ÜNİVERSİTESİ", CityPlateCode = 61, IsActive = true },
            new University { Code = 1143, Name = "TRAKYA ÜNİVERSİTESİ", CityPlateCode = 22, IsActive = true },
            new University { Code = 1144, Name = "TÜRK HAVA KURUMU ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1145, Name = "TÜRK-ALMAN ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1146, Name = "UFUK ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1147, Name = "UŞAK ÜNİVERSİTESİ", CityPlateCode = 64, IsActive = true },
            new University { Code = 1148, Name = "VAN YÜZÜNCÜ YIL ÜNİVERSİTESİ", CityPlateCode = 65, IsActive = true },
            new University { Code = 1149, Name = "YALOVA ÜNİVERSİTESİ", CityPlateCode = 77, IsActive = true },
            new University { Code = 1150, Name = "YAŞAR ÜNİVERSİTESİ", CityPlateCode = 35, IsActive = true },
            new University { Code = 1151, Name = "YEDİTEPE ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1152, Name = "YILDIZ TEKNİK ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1153, Name = "YOZGAT BOZOK ÜNİVERSİTESİ", CityPlateCode = 66, IsActive = true },
            new University { Code = 1154, Name = "YÜKSEK İHTİSAS ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1155, Name = "ZONGULDAK BÜLENT ECEVİT ÜNİVERSİTESİ", CityPlateCode = 67, IsActive = true },
            new University { Code = 1156, Name = "ÇANAKKALE ONSEKİZ MART ÜNİVERSİTESİ", CityPlateCode = 17, IsActive = true },
            new University { Code = 1157, Name = "ÇANKAYA ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1158, Name = "ÇANKIRI KARATEKİN ÜNİVERSİTESİ", CityPlateCode = 18, IsActive = true },
            new University { Code = 1159, Name = "ÇAĞ ÜNİVERSİTESİ", CityPlateCode = 33, IsActive = true },
            new University { Code = 1160, Name = "ÇUKUROVA ÜNİVERSİTESİ", CityPlateCode = 1, IsActive = true },
            new University { Code = 1161, Name = "ÖZYEĞİN ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1162, Name = "ÜSKÜDAR ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1163, Name = "İBN HALDUN ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1164, Name = "İHSAN DOĞRAMACI BİLKENT ÜNİVERSİTESİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1165, Name = "İNÖNÜ ÜNİVERSİTESİ", CityPlateCode = 44, IsActive = true },
            new University { Code = 1166, Name = "İSKENDERUN TEKNİK ÜNİVERSİTESİ", CityPlateCode = 31, IsActive = true },
            new University { Code = 1167, Name = "İSTANBUL 29 MAYIS ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1168, Name = "İSTANBUL AREL ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1169, Name = "İSTANBUL ATLAS ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1170, Name = "İSTANBUL AYDIN ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1171, Name = "İSTANBUL BEYKENT ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1172, Name = "İSTANBUL BİLGİ ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1173, Name = "İSTANBUL ESENYURT ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1174, Name = "İSTANBUL GALATA ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1175, Name = "İSTANBUL GEDİK ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1176, Name = "İSTANBUL GELİŞİM ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1177, Name = "İSTANBUL KENT ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1178, Name = "İSTANBUL KÜLTÜR ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1179, Name = "İSTANBUL MEDENİYET ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1180, Name = "İSTANBUL MEDİPOL ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1181, Name = "İSTANBUL NİŞANTAŞI ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1182, Name = "İSTANBUL OKAN ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1183, Name = "İSTANBUL RUMELİ ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1184, Name = "İSTANBUL SABAHATTİN ZAİM ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1185, Name = "İSTANBUL SAĞLIK VE TEKNOLOJİ ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1186, Name = "İSTANBUL TEKNİK ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1187, Name = "İSTANBUL TOPKAPI ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1188, Name = "İSTANBUL TİCARET ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1189, Name = "İSTANBUL YENİ YÜZYIL ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1190, Name = "İSTANBUL ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1191, Name = "İSTANBUL ÜNİVERSİTESİ-CERRAHPAŞA", CityPlateCode = 34, IsActive = true },
            new University { Code = 1192, Name = "İSTİNYE ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1193, Name = "İZMİR BAKIRÇAY ÜNİVERSİTESİ", CityPlateCode = 35, IsActive = true },
            new University { Code = 1194, Name = "İZMİR DEMOKRASİ ÜNİVERSİTESİ", CityPlateCode = 35, IsActive = true },
            new University { Code = 1195, Name = "İZMİR EKONOMİ ÜNİVERSİTESİ", CityPlateCode = 35, IsActive = true },
            new University { Code = 1196, Name = "İZMİR KATİP ÇELEBİ ÜNİVERSİTESİ", CityPlateCode = 35, IsActive = true },
            new University { Code = 1197, Name = "İZMİR TINAZTEPE ÜNİVERSİTESİ", CityPlateCode = 35, IsActive = true },
            new University { Code = 1198, Name = "İZMİR YÜKSEK TEKNOLOJİ ENSTİTÜSÜ", CityPlateCode = 35, IsActive = true },
            new University { Code = 1199, Name = "ŞIRNAK ÜNİVERSİTESİ", CityPlateCode = 73, IsActive = true },
            new University { Code = 1200, Name = "IŞIK ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1201, Name = "JANDARMA VE SAHİL GÜVENLİK AKADEMİSİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1202, Name = "MİLLİ SAVUNMA ÜNİVERSİTESİ", CityPlateCode = 34, IsActive = true },
            new University { Code = 1203, Name = "POLİS AKADEMİSİ", CityPlateCode = 6, IsActive = true },
            new University { Code = 1204, Name = "ATAŞEHİR ADIGÜZEL MESLEK YÜKSEKOKULU", CityPlateCode = 34, IsActive = true },
            new University { Code = 1205, Name = "İSTANBUL SAĞLIK VE SOSYAL BİLİMLER MESLEK YÜKSEKOKULU", CityPlateCode = 34, IsActive = true },
            new University { Code = 1206, Name = "İSTANBUL ŞİŞLİ MESLEK YÜKSEKOKULU", CityPlateCode = 34, IsActive = true },
            new University { Code = 1207, Name = "İZMİR KONAK MESLEK YÜKSEKOKULU", CityPlateCode = 35, IsActive = true },
        };
    }
}
