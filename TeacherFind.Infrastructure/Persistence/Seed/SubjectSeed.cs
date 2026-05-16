using Microsoft.EntityFrameworkCore;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Persistence.Seed;

// ┌──────────────────────────────────────────────────────────────────────┐
// │  3-LEVEL TREE STRUCTURE                                              │
// │  Category Root  (9001–9032)  e.g. "Matematik"                        │
// │  └── Level Node (9100–9412)  e.g. "İlkokul"                          │
// │       └── Leaf  (orig code)  e.g. "Genel Matematik"                  │
// │                                                                      │
// │  Only levels that exist in the data are added per category.          │
// │  Add to Subject entity:                                              │
// │      public Guid? ParentId { get; set; }                             │
// │      public Subject? Parent  { get; set; }                           │
// └──────────────────────────────────────────────────────────────────────┘

public static class SubjectSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var existingByCodes = await context.Subjects
            .Where(x => x.Code > 0)
            .GroupBy(x => x.Code)
            .ToDictionaryAsync(g => g.Key, g => g.First());

        foreach (var seed in GetSubjects())
        {
            var stage = DeriveStage(seed.Level);
            if (existingByCodes.TryGetValue(seed.Code, out var subject))
            {
                subject.Category = seed.Category; subject.Name = seed.Name;
                subject.Level = seed.Level; subject.Stage = stage;
                subject.IsActive = seed.IsActive;
                continue;
            }
            subject = new Subject
            {
                Code = seed.Code,
                Category = seed.Category,
                Name = seed.Name,
                Level = seed.Level,
                Stage = stage,
                IsActive = seed.IsActive
            };
            await context.Subjects.AddAsync(subject);
            existingByCodes[seed.Code] = subject;
        }
        await context.SaveChangesAsync();

        foreach (var seed in GetSubjects().Where(s => s.ParentCode.HasValue))
            if (existingByCodes.TryGetValue(seed.Code, out var s) &&
                existingByCodes.TryGetValue(seed.ParentCode!.Value, out var p))
                s.ParentId = p.Id;

        await context.SaveChangesAsync();
    }

    private static string DeriveStage(string level) => level switch
    {
        "İlkokul" => "İlk-Ortaöğretim",
        "Ortaokul" => "İlk-Ortaöğretim",
        "Lise" => "Lise",
        "Üniversite" => "Üniversite",
        "Yetişkin" => "Yetişkin",
        _ => "Her Seviye"
    };

    private static List<SubjectSeedItem> GetSubjects()
    {
        var list = new List<SubjectSeedItem>();
        list.AddRange(GetMatematik()); list.AddRange(GetFizik());
        list.AddRange(GetKimya()); list.AddRange(GetBiyoloji());
        list.AddRange(GetFenBilimleri()); list.AddRange(GetTurkceEdebiyat());
        list.AddRange(GetSosyalBilgiler()); list.AddRange(GetTarih());
        list.AddRange(GetCografya()); list.AddRange(GetFelsefe());
        list.AddRange(GetPsikoloji()); list.AddRange(GetIstatistik());
        list.AddRange(GetEkonomiHukuk()); list.AddRange(GetYabanciDiller());
        list.AddRange(GetTurkiyeSinavlari());
        list.AddRange(GetUluslararasiSinavlar());
        list.AddRange(GetAlmancaSinavlari());
        list.AddRange(GetDilSertifikalari());
        list.AddRange(GetTemelEgitim()); list.AddRange(GetOzelEgitim());
        list.AddRange(GetUniversiteTakviye());
        list.AddRange(GetMuzik()); list.AddRange(GetSpor());
        list.AddRange(GetDans()); list.AddRange(GetSanat());
        list.AddRange(GetBilisim()); list.AddRange(GetRobotik());
        list.AddRange(GetDanismanlik()); list.AddRange(GetMuhasebe());
        list.AddRange(GetKisiselGelisim()); list.AddRange(GetSaglik());
        list.AddRange(GetHobi());
        return list;
    }

    // ParentCode = null  → root entry (no parent)
    // ParentCode = X     → child of entry with Code X
    private sealed record SubjectSeedItem(
        int Code, string Category, string Name,
        string Level, bool IsActive, int? ParentCode = null);

    // ════════════════════════════════════════════════════════════════
    //  MATEMATİK  root 9001
    //  İlkokul 9101 / Ortaokul 9102 / Lise 9103 / Üniversite 9104
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetMatematik()
    {
        const string C = "Matematik";
        return
        [
            new(9001, C, "Matematik",   "Her Seviye", true),
            new(9101, C, "İlkokul",    "İlkokul",    true, 9001),
            new(9102, C, "Ortaokul",   "Ortaokul",   true, 9001),
            new(9103, C, "Lise",       "Lise",       true, 9001),
            new(9104, C, "Üniversite", "Üniversite", true, 9001),
            // İlkokul
            new(  1, C, "Matematik",         "İlkokul",    true, 9101),
            // Ortaokul
            new(  4, C, "Genel Matematik",         "Ortaokul",   true, 9102),
            new(  5, C, "Geometri",                "Ortaokul",   true, 9102),
            new(  6, C, "Olasılık ve İstatistik", "Ortaokul",   true, 9102),
            new(  7, C, "Cebir",                   "Ortaokul",   true, 9102),
            // Lise
            new(  8, C, "Genel Matematik",         "Lise",       true, 9103),
            new(  9, C, "Geometri",                "Lise",       true, 9103),
            new( 10, C, "Analitik Geometri",       "Lise",       true, 9103),
            new( 11, C, "Trigonometri",            "Lise",       true, 9103),
            new( 12, C, "Olasılık ve İstatistik", "Lise",       true, 9103),
            new( 13, C, "Calculus (Temel)",        "Lise",       true, 9103),
            new( 14, C, "Sayılar Teorisi",         "Lise",       true, 9103),
            // Üniversite
            new( 15, C, "Calculus / Analiz",         "Üniversite", true, 9104),
            new( 16, C, "Diferansiyel Denklemler",   "Üniversite", true, 9104),
            new( 17, C, "Lineer Cebir",              "Üniversite", true, 9104),
            new( 18, C, "Matematiksel Modelleme",    "Üniversite", true, 9104),
            new( 19, C, "Ayrık Matematik",           "Üniversite", true, 9104),
            new( 20, C, "Yöneylem Araştırması",      "Üniversite", true, 9104),
            new( 21, C, "İleri Olasılık Teorisi",   "Üniversite", true, 9104),
        ];
    }


    // ════════════════════════════════════════════════════════════════
    //  FİZİK  root 9002
    //  Lise 9111 / Üniversite 9112
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetFizik()
    {
        const string C = "Fizik";
        return
        [
            new(9002, C, "Fizik",      "Her Seviye", true),
            new(9111, C, "Lise",       "Lise",       true, 9002),
            new(9112, C, "Üniversite", "Üniversite", true, 9002),
            new(470, C, "Genel Fizik",       "Lise",       true, 9111),
            new(471, C, "Mekanik",           "Lise",       true, 9111),
            new(472, C, "Elektromanyetizma", "Lise",       true, 9111),
            new(473, C, "Optik",             "Lise",       true, 9111),
            new(474, C, "Genel Fizik",          "Üniversite", true, 9112),
            new(475, C, "Mekanik",              "Üniversite", true, 9112),
            new(476, C, "Termodinamik",         "Üniversite", true, 9112),
            new(477, C, "Elektromanyetizma",    "Üniversite", true, 9112),
            new(478, C, "Kuantum Fiziği",       "Üniversite", true, 9112),
            new(479, C, "Akışkanlar Mekaniği", "Üniversite", true, 9112),
            new(480, C, "Mukavemet",            "Üniversite", true, 9112),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  KİMYA  root 9003
    //  Lise 9121 / Üniversite 9122
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetKimya()
    {
        const string C = "Kimya";
        return
        [
            new(9003, C, "Kimya",      "Her Seviye", true),
            new(9121, C, "Lise",       "Lise",       true, 9003),
            new(9122, C, "Üniversite", "Üniversite", true, 9003),
            new(490, C, "Genel Kimya",    "Lise",       true, 9121),
            new(492, C, "Organik Kimya",  "Lise",       true, 9121),
            new(491, C, "Genel Kimya",    "Üniversite", true, 9122),
            new(493, C, "Organik Kimya",  "Üniversite", true, 9122),
            new(494, C, "Analitik Kimya", "Üniversite", true, 9122),
            new(495, C, "Biyokimya",      "Üniversite", true, 9122),
            new(496, C, "Fizikokimya",    "Üniversite", true, 9122),
            new(497, C, "Çevre Kimyası", "Üniversite", true, 9122),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  BİYOLOJİ  root 9004
    //  Lise 9131 / Üniversite 9132
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetBiyoloji()
    {
        const string C = "Biyoloji";
        return
        [
            new(9004, C, "Biyoloji",   "Her Seviye", true),
            new(9131, C, "Lise",       "Lise",       true, 9004),
            new(9132, C, "Üniversite", "Üniversite", true, 9004),
            new(510, C, "Genel Biyoloji",    "Lise",       true, 9131),
            new(512, C, "Genetik",           "Lise",       true, 9131),
            new(511, C, "Genel Biyoloji",    "Üniversite", true, 9132),
            new(513, C, "Genetik",           "Üniversite", true, 9132),
            new(514, C, "Hücre Biyolojisi", "Üniversite", true, 9132),
            new(515, C, "Mikrobiyoloji",     "Üniversite", true, 9132),
            new(516, C, "Fizyoloji",         "Üniversite", true, 9132),
            new(517, C, "Anatomi",           "Üniversite", true, 9132),
            new(518, C, "Ekoloji",           "Üniversite", true, 9132),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  FEN BİLİMLERİ  root 9005
    //  İlkokul 9141 / Ortaokul 9142
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetFenBilimleri()
    {
        const string C = "Fen Bilimleri";
        return
        [
            new(9005, C, "Fen Bilimleri", "Her Seviye", true),
            new(9141, C, "İlkokul",       "İlkokul",    true, 9005),
            new(9142, C, "Ortaokul",      "Ortaokul",   true, 9005),
            new(450, C, "Genel Fen Bilgisi",    "İlkokul",  true, 9141),
            new(451, C, "Genel Fen Bilimleri",  "Ortaokul", true, 9142),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  TÜRKÇE VE EDEBİYAT  root 9006
    //  İlkokul 9151 / Ortaokul 9152 / Lise 9153 / Üniversite 9154 / Her Seviye 9155
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetTurkceEdebiyat()
    {
        const string C = "Türkçe ve Edebiyat";
        return
        [
            new(9006, C, "Türkçe ve Edebiyat", "Her Seviye", true),
            new(9151, C, "İlkokul",    "İlkokul",    true, 9006),
            new(9152, C, "Ortaokul",   "Ortaokul",   true, 9006),
            new(9153, C, "Lise",       "Lise",       true, 9006),
            new(9154, C, "Üniversite", "Üniversite", true, 9006),
            new(9155, C, "Her Seviye", "Her Seviye", true, 9006),
            new(530, C, "Türkçe",                     "İlkokul",    true, 9151),
            new(531, C, "Türkçe",                     "Ortaokul",   true, 9152),
            new(532, C, "Dil ve Anlatım",             "Lise",       true, 9153),
            new(533, C, "Türk Dili ve Edebiyatı",     "Lise",       true, 9153),
            new(534, C, "Türk Dili ve Edebiyatı",     "Üniversite", true, 9154),
            new(535, C, "Osmanlı Türkçesi",           "Üniversite", true, 9154),
            new(536, C, "Diksiyon",                   "Her Seviye", true, 9155),
            new(537, C, "Yaratıcı Yazarlık",          "Her Seviye", true, 9155),
            new(538, C, "Hızlı Okuma Teknikleri",    "Her Seviye", true, 9155),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  SOSYAL BİLGİLER  root 9007
    //  İlkokul 9161 / Ortaokul 9162
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetSosyalBilgiler()
    {
        const string C = "Sosyal Bilgiler";
        return
        [
            new(9007, C, "Sosyal Bilgiler", "Her Seviye", true),
            new(9161, C, "İlkokul",         "İlkokul",    true, 9007),
            new(9162, C, "Ortaokul",        "Ortaokul",   true, 9007),
            new(552, C, "Hayat Bilgisi",       "İlkokul",  true, 9161),
            new(550, C, "Sosyal Bilgiler",     "Ortaokul", true, 9162),
            new(551, C, "Vatandaşlık Bilgisi", "Ortaokul", true, 9162),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  TARİH  root 9008
    //  Lise 9171 / Üniversite 9172
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetTarih()
    {
        const string C = "Tarih";
        return
        [
            new(9008, C, "Tarih",      "Her Seviye", true),
            new(9171, C, "Lise",       "Lise",       true, 9008),
            new(9172, C, "Üniversite", "Üniversite", true, 9008),
            new(560, C, "Türk Tarihi",                     "Lise",       true, 9171),
            new(561, C, "Dünya Tarihi",                    "Lise",       true, 9171),
            new(562, C, "Osmanlı Tarihi",                  "Lise",       true, 9171),
            new(563, C, "İnkılap Tarihi ve Atatürkçülük", "Lise",       true, 9171),
            new(564, C, "Türk Tarihi",                     "Üniversite", true, 9172),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  COĞRAFYA  root 9009
    //  Lise 9181
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetCografya()
    {
        const string C = "Coğrafya";
        return
        [
            new(9009, C, "Coğrafya", "Her Seviye", true),
            new(9181, C, "Lise",     "Lise",       true, 9009),
            new(570, C, "Coğrafya",        "Lise", true, 9181),

        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  FELSEFE  root 9010
    //  Lise 9191 / Üniversite 9192
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetFelsefe()
    {
        const string C = "Felsefe";
        return
        [
            new(9010, C, "Felsefe",    "Her Seviye", true),
            new(9191, C, "Lise",       "Lise",       true, 9010),
            new(9192, C, "Üniversite", "Üniversite", true, 9010),
            new(580, C, "Genel Felsefe",          "Lise",       true, 9191),
            new(581, C, "Genel Felsefe",          "Üniversite", true, 9192),
            new(582, C, "Epistemoloji",           "Üniversite", true, 9192),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  PSİKOLOJİ  root 9011
    //  Lise 9201 / Üniversite 9202
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetPsikoloji()
    {
        const string C = "Psikoloji";
        return
        [
            new(9011, C, "Psikoloji",  "Her Seviye", true),
            new(9201, C, "Lise",       "Lise",       true, 9011),
            new(9202, C, "Üniversite", "Üniversite", true, 9011),
            new(590, C, "Genel Psikoloji",      "Lise",       true, 9201),
            new(591, C, "Genel Psikoloji",      "Üniversite", true, 9202),
            new(592, C, "Sosyal Psikoloji",     "Üniversite", true, 9202),
            new(593, C, "Gelişim Psikolojisi",  "Üniversite", true, 9202),
            new(594, C, "Klinik Psikoloji",     "Üniversite", true, 9202),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  İSTATİSTİK  root 9012
    //  Lise 9211 / Üniversite 9212
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetIstatistik()
    {
        const string C = "İstatistik";
        return
        [
            new(9012, C, "İstatistik", "Her Seviye", true),
            new(9211, C, "Lise",       "Lise",       true, 9012),
            new(9212, C, "Üniversite", "Üniversite", true, 9012),
            new(600, C, "Genel İstatistik", "Lise",       true, 9211),
            new(601, C, "Genel İstatistik", "Üniversite", true, 9212),
            new(602, C, "Olasılık Teorisi", "Üniversite", true, 9212),
            new(603, C, "SPSS",             "Üniversite", true, 9212),
            new(604, C, "Veri Analizi",     "Üniversite", true, 9212),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  EKONOMİ VE HUKUK  root 9013
    //  Lise 9221 / Üniversite 9222
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetEkonomiHukuk()
    {
        const string C = "Ekonomi ve Hukuk";
        return
        [
            new(9013, C, "Ekonomi ve Hukuk", "Her Seviye", true),
            new(9221, C, "Lise",             "Lise",       true, 9013),
            new(9222, C, "Üniversite",       "Üniversite", true, 9013),
            new(614, C, "İşletme",        "Lise",       true, 9221),
            new(610, C, "Mikroekonomi",   "Üniversite", true, 9222),
            new(611, C, "Makroekonomi",   "Üniversite", true, 9222),
            new(612, C, "Ekonometri",     "Üniversite", true, 9222),
            new(613, C, "Maliye",         "Üniversite", true, 9222),
            new(615, C, "İşletme",        "Üniversite", true, 9222),
            new(620, C, "Özel Hukuk",     "Üniversite", true, 9222),
            new(621, C, "Kamu Hukuku",    "Üniversite", true, 9222),
            new(622, C, "Ticaret Hukuku", "Üniversite", true, 9222),
            new(623, C, "İş Hukuku",      "Üniversite", true, 9222),
            new(624, C, "Ceza Hukuku",    "Üniversite", true, 9222),
            new(625, C, "Anayasa Hukuku", "Üniversite", true, 9222),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  YABANCI DİLLER  root 9014
    //  İlkokul 9231 / Ortaokul 9232 / Lise 9233
    //  Üniversite 9234 / Yetişkin 9235 / Her Seviye 9236
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetYabanciDiller()
    {
        const string C = "Yabancı Diller";
        return
        [
            new(9014, C, "Yabancı Diller", "Her Seviye", true),
            new(9231, C, "İlkokul",    "İlkokul",    true, 9014),
            new(9232, C, "Ortaokul",   "Ortaokul",   true, 9014),
            new(9233, C, "Lise",       "Lise",       true, 9014),
            new(9234, C, "Üniversite", "Üniversite", true, 9014),
            new(9235, C, "Yetişkin",   "Yetişkin",   true, 9014),
            new(9236, C, "Her Seviye", "Her Seviye", true, 9014),
            new(400, C, "İngilizce",                   "İlkokul",    true, 9231),
            new(401, C, "İngilizce",                   "Ortaokul",   true, 9232),
            new(402, C, "İngilizce",                   "Lise",       true, 9233),
            new(403, C, "İngilizce",                   "Üniversite", true, 9234),
            new(425, C, "İngiliz Dili ve Edebiyatı",  "Üniversite", true, 9234),
            new(404, C, "İngilizce",                   "Yetişkin",   true, 9235),
            new(405, C, "Almanca",              "Her Seviye", true, 9236),
            new(406, C, "Fransızca",            "Her Seviye", true, 9236),
            new(407, C, "Rusça",                "Her Seviye", true, 9236),
            new(408, C, "Arapça",               "Her Seviye", true, 9236),
            new(409, C, "İspanyolca",           "Her Seviye", true, 9236),
            new(410, C, "İtalyanca",            "Her Seviye", true, 9236),
            new(411, C, "Japonca",              "Her Seviye", true, 9236),
            new(412, C, "Çince (Mandarin)",     "Her Seviye", true, 9236),
            new(413, C, "Portekizce",           "Her Seviye", true, 9236),
            new(414, C, "Korece",               "Her Seviye", true, 9236),
            new(415, C, "Farsça",               "Her Seviye", true, 9236),
            new(416, C, "Yunanca",              "Her Seviye", true, 9236),
            new(417, C, "Hollandaca",           "Her Seviye", true, 9236),
            new(418, C, "Norveçce",             "Her Seviye", true, 9236),
            new(419, C, "Lehçe",                "Her Seviye", true, 9236),
            new(420, C, "Ukraynaca",            "Her Seviye", true, 9236),
            new(421, C, "Latince",              "Her Seviye", true, 9236),
            new(422, C, "Hintçe (Hindice)",     "Her Seviye", true, 9236),
            new(423, C, "İbranice",             "Her Seviye", true, 9236),
            new(424, C, "Türkçe (Yabancılara)","Her Seviye", true, 9236),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  TÜRKİYE SINAVLARI  root 9015
    //  Ortaokul 9241 / Lise 9242 / Üniversite 9243 / Yetişkin 9244
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetTurkiyeSinavlari()
    {
        const string C = "Türkiye Sınavları";
        return
        [
            new(9015, C, "Türkiye Sınavları", "Her Seviye", true),
            new(9241, C, "Ortaokul",   "Ortaokul",   true, 9015),
            new(9242, C, "Lise",       "Lise",       true, 9015),
            new(9243, C, "Üniversite", "Üniversite", true, 9015),
            new(9244, C, "Yetişkin",   "Yetişkin",   true, 9015),
            // Ortaokul
            new( 93, C, "LGS",                            "Ortaokul", true, 9241),
            new(100, C, "Genel Hazırlık",                 "Ortaokul", true,   93),
            new(101, C, "Matematik",                       "Ortaokul", true,   93),
            new(102, C, "Türkçe",                          "Ortaokul", true,   93),
            new(103, C, "Fen Bilimleri",                  "Ortaokul", true,   93),
            new(104, C, "İnkılap Tarihi ve Din Kültürü", "Ortaokul", true,   93),
            new(105, C, "İOKBS – Bursluluk Sınavı",      "Ortaokul", true, 9241),
            // Lise
            new( 94, C, "YKS",                                   "Lise", true, 9242),
            new(110, C, "Genel Hazırlık (TYT + AYT)",           "Lise", true,   94),
            new(111, C, "TYT – Temel Yeterlilik Testi",         "Lise", true,   94),
            new(112, C, "AYT – Sayısal",                         "Lise", true,   94),
            new(113, C, "AYT – Eşit Ağırlık",                  "Lise", true,   94),
            new(114, C, "AYT – Sözel",                           "Lise", true,   94),
            new(115, C, "YDT – Yabancı Dil Testi",             "Lise", true,   94),
            new(116, C, "MSÜ – Milli Savunma Üniversitesi",     "Lise", true, 9242),
            new(117, C, "Polis Akademisi Giriş Sınavı (PAFS)", "Lise", true, 9242),
            // Üniversite
            new(140, C, "ALES – Akademik Personel ve Lisansüstü", "Üniversite", true, 9243),
            new(141, C, "DGS – Dikey Geçiş Sınavı",              "Üniversite", true, 9243),
            new(145, C, "AÖF – Açıköğretim Sınavları",           "Üniversite", true, 9243),
            // Yetişkin
            new( 95, C, "YDS",        "Yetişkin", true, 9244),
            new(120, C, "İngilizce",  "Yetişkin", true,   95),
            new(121, C, "Almanca",    "Yetişkin", true,   95),
            new(122, C, "Fransızca",  "Yetişkin", true,   95),
            new(123, C, "Arapça",     "Yetişkin", true,   95),
            new(124, C, "Rusça",      "Yetişkin", true,   95),
            new(125, C, "İspanyolca", "Yetişkin", true,   95),
            new(126, C, "İtalyanca",  "Yetişkin", true,   95),
            new(127, C, "Japonca",    "Yetişkin", true,   95),
            new(128, C, "Çince",      "Yetişkin", true,   95),
            new(129, C, "Portekizce", "Yetişkin", true,   95),
            new( 96, C, "YÖKDİL",    "Yetişkin", true, 9244),
            new(130, C, "İngilizce",  "Yetişkin", true,   96),
            new(131, C, "Almanca",    "Yetişkin", true,   96),
            new(132, C, "Fransızca",  "Yetişkin", true,   96),
            new(133, C, "Arapça",     "Yetişkin", true,   96),
            new(134, C, "Rusça",      "Yetişkin", true,   96),
            new(135, C, "İspanyolca", "Yetişkin", true,   96),
            new(136, C, "İtalyanca",  "Yetişkin", true,   96),
            new(137, C, "Japonca",    "Yetişkin", true,   96),
            new(138, C, "Çince",      "Yetişkin", true,   96),
            new(139, C, "Portekizce", "Yetişkin", true,   96),
            new( 97, C, "KPSS",       "Yetişkin", true, 9244),
            new(150, C, "Genel Yetenek / Genel Kültür",       "Yetişkin", true, 97),
            new(151, C, "Eğitim Bilimleri",                   "Yetişkin", true, 97),
            new(152, C, "ÖABT – Öğretmenlik Alan Bilgisi",    "Yetişkin", true, 97),
            new(153, C, "EKYS – Eğitim Kurumları Yöneticisi", "Yetişkin", true, 97),
            new(142, C, "TUS – Tıpta Uzmanlık Sınavı",                   "Yetişkin", true, 9244),
            new(143, C, "DUS – Diş Hekimliği Uzmanlık Sınavı",           "Yetişkin", true, 9244),
            new(144, C, "Eczacılık Uzmanlık Sınavı",                     "Yetişkin", true, 9244),
            new(154, C, "Hakimlik ve Savcılık Sınavı",                   "Yetişkin", true, 9244),
            new(155, C, "Avukatlık (Baro) Sınavı",                       "Yetişkin", true, 9244),
            new(156, C, "Noterlik Sınavı",                               "Yetişkin", true, 9244),
            new(157, C, "Uzman Erbaş Sınavı",                            "Yetişkin", true, 9244),
            new(158, C, "Subay / Astsubay Sınavları",                    "Yetişkin", true, 9244),
            new(159, C, "Vergi Müfettişi Sınavı",                        "Yetişkin", true, 9244),
            new(160, C, "Gümrük Müfettişi Sınavı",                       "Yetişkin", true, 9244),
            new(165, C, "SMMM – Serbest Muhasebeci Mali Müşavirlik",     "Yetişkin", true, 9244),
            new(166, C, "YMM – Yeminli Mali Müşavirlik",                 "Yetişkin", true, 9244),
            new(167, C, "İSG – İş Sağlığı ve Güvenliği Uzmanlığı",       "Yetişkin", true, 9244),
            new(168, C, "SPK – Sermaye Piyasası Lisanslama",             "Yetişkin", true, 9244),
            new(169, C, "Aktüerlik Sınavı",                              "Yetişkin", true, 9244),
            new(170, C, "MYK – Mesleki Yeterlilik Sınavları",            "Yetişkin", true, 9244),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  ULUSLARARASI SINAVLAR  root 9016
    //  Her Seviye 9251 / Lise 9252 / Üniversite 9253 / Yetişkin 9254
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetUluslararasiSinavlar()
    {
        const string C = "Uluslararası Sınavlar";
        return
        [
            new(9016, C, "Uluslararası Sınavlar", "Her Seviye", true),
            new(9251, C, "Her Seviye", "Her Seviye", true, 9016),
            new(9252, C, "Lise",       "Lise",       true, 9016),
            new(9253, C, "Üniversite", "Üniversite", true, 9016),
            new(9254, C, "Yetişkin",   "Yetişkin",   true, 9016),
            new(200, C, "IELTS Academic",                 "Her Seviye", true, 9251),
            new(201, C, "IELTS General Training",         "Her Seviye", true, 9251),
            new(202, C, "TOEFL iBT",                      "Her Seviye", true, 9251),
            new(203, C, "Cambridge B2 First (FCE)",       "Her Seviye", true, 9251),
            new(204, C, "Cambridge C1 Advanced (CAE)",    "Her Seviye", true, 9251),
            new(205, C, "Cambridge C2 Proficiency (CPE)", "Her Seviye", true, 9251),
            new(206, C, "PTE Academic",                   "Her Seviye", true, 9251),
            new(207, C, "Duolingo English Test",          "Her Seviye", true, 9251),
            new(210, C, "ITEP",                           "Her Seviye", true, 9251),
            new(215, C, "SAT – Matematik ",                "Lise", true, 9252),
            new(216, C, "SAT – Okuma-Yazma",                "Lise", true, 9252),
            new(217, C, "ACT",                             "Lise", true, 9252),
            new(225, C, "IB – International Baccalaureate","Lise", true, 9252),
            new(226, C, "IGCSE",                           "Lise", true, 9252),
            new(227, C, "A-Levels",                        "Lise", true, 9252),
            new(228, C, "AP – Advanced Placement",         "Lise", true, 9252),
            new(217, C, "GRE – Graduate Record Exam",    "Üniversite", true, 9253),
            new(218, C, "GMAT – İşletme Yüksek Lisans", "Üniversite", true, 9253),
            new(219, C, "LSAT – Hukuk Okulu Giriş",     "Üniversite", true, 9253),
            new(220, C, "MCAT – Tıp Okulu Giriş",       "Üniversite", true, 9253),
            new(208, C, "TOEIC – İş İngilizcesi",        "Yetişkin", true, 9254),
            new(209, C, "OET – Occupational English Test","Yetişkin", true, 9254),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  ALMANCA SINAVLARI  root 9017
    //  Her Seviye 9261 / Üniversite 9262 / Yetişkin 9263 / Lise 9264
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetAlmancaSinavlari()
    {
        const string C = "Almanca Sınavları";
        return
        [
            new(9017, C, "Almanca Sınavları", "Her Seviye", true),
            new(9261, C, "Her Seviye", "Her Seviye", true, 9017),
            new(9262, C, "Üniversite", "Üniversite", true, 9017),
            new(9263, C, "Yetişkin",   "Yetişkin",   true, 9017),
            new(9264, C, "Lise",       "Lise",       true, 9017),
            new(250, C, "Goethe A1 – Start Deutsch 1",               "Her Seviye", true, 9261),
            new(251, C, "Goethe A2",                                  "Her Seviye", true, 9261),
            new(252, C, "Goethe B1",                                  "Her Seviye", true, 9261),
            new(253, C, "Goethe B2",                                  "Her Seviye", true, 9261),
            new(254, C, "Goethe C1",                                  "Her Seviye", true, 9261),
            new(255, C, "Goethe C2 – Großes Deutsches Sprachdiplom", "Her Seviye", true, 9261),
            new(265, C, "Telc Deutsch A1",                            "Her Seviye", true, 9261),
            new(266, C, "Telc Deutsch A2",                            "Her Seviye", true, 9261),
            new(267, C, "Telc Deutsch B1",                            "Her Seviye", true, 9261),
            new(268, C, "Telc Deutsch B2",                            "Her Seviye", true, 9261),
            new(269, C, "Telc Deutsch C1",                            "Her Seviye", true, 9261),
            new(275, C, "ÖSD – Österreichisches Sprachdiplom Deutsch","Her Seviye", true, 9261),
            new(278, C, "Almanca Vizesi Dil Sınavı (A1)",            "Her Seviye", true, 9261),
            new(260, C, "TestDaF – Test Deutsch als Fremdsprache",                "Üniversite", true, 9262),
            new(261, C, "DSH – Deutsche Sprachprüfung für den Hochschulzugang",  "Üniversite", true, 9262),
            new(270, C, "Telc Deutsch B1+ Beruf – İş Almancası", "Yetişkin", true, 9263),
            new(271, C, "Telc Deutsch B2+ Beruf – İş Almancası", "Yetişkin", true, 9263),
            new(276, C, "DSD – Deutsches Sprachdiplom (Okul)", "Lise", true, 9264),
            new(277, C, "Abitur Hazırlık",                      "Lise", true, 9264),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  DİL SERTİFİKA SINAVLARI  root 9018
    //  Her Seviye 9271 / İlkokul 9272
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetDilSertifikalari()
    {
        const string C = "Dil Sertifika Sınavları";
        return
        [
            new(9018, C, "Dil Sertifika Sınavları", "Her Seviye", true),
            new(9271, C, "Her Seviye", "Her Seviye", true, 9018),
            new(9272, C, "İlkokul",    "İlkokul",    true, 9018),
            new(300, C, "DELF A1–A2 (Fransızca)",               "Her Seviye", true, 9271),
            new(301, C, "DELF B1–B2 (Fransızca)",               "Her Seviye", true, 9271),
            new(302, C, "DALF C1–C2 (Fransızca)",               "Her Seviye", true, 9271),
            new(303, C, "TCF – Test de Connaissance du Français","Her Seviye", true, 9271),
            new(304, C, "TEF – Test d'Évaluation de Français",  "Her Seviye", true, 9271),
            new(310, C, "DELE A1–A2 (İspanyolca)",              "Her Seviye", true, 9271),
            new(311, C, "DELE B1–B2 (İspanyolca)",              "Her Seviye", true, 9271),
            new(312, C, "DELE C1–C2 (İspanyolca)",              "Her Seviye", true, 9271),
            new(313, C, "SIELE – İspanyolca Çevrimiçi",         "Her Seviye", true, 9271),
            new(318, C, "CILS – Certificazione İtalyanca",       "Her Seviye", true, 9271),
            new(319, C, "CELI – Certificato İtalyanca",          "Her Seviye", true, 9271),
            new(320, C, "PLIDA – İtalyanca Yeterlilik",          "Her Seviye", true, 9271),
            new(325, C, "CELPE-BRAS (Portekizce – Brezilya)",    "Her Seviye", true, 9271),
            new(330, C, "JLPT N5 – Japonca Başlangıç",          "Her Seviye", true, 9271),
            new(331, C, "JLPT N4",                               "Her Seviye", true, 9271),
            new(332, C, "JLPT N3",                               "Her Seviye", true, 9271),
            new(333, C, "JLPT N2",                               "Her Seviye", true, 9271),
            new(334, C, "JLPT N1 – Japonca İleri",              "Her Seviye", true, 9271),
            new(338, C, "HSK 1–2 (Çince Başlangıç)",           "Her Seviye", true, 9271),
            new(339, C, "HSK 3–4 (Çince Orta)",                "Her Seviye", true, 9271),
            new(340, C, "HSK 5–6 (Çince İleri)",              "Her Seviye", true, 9271),
            new(341, C, "HSKK – Çince Konuşma Sınavı",         "Her Seviye", true, 9271),
            new(345, C, "TOPIK I (Korece 1–2. Seviye)",         "Her Seviye", true, 9271),
            new(346, C, "TOPIK II (Korece 3–6. Seviye)",        "Her Seviye", true, 9271),
            new(350, C, "TRKI / TORFL – Rusça Yeterlilik",      "Her Seviye", true, 9271),
            new(354, C, "ALPT – Arapça Dil Yeterlilik Sınavı",  "Her Seviye", true, 9271),
            new(359, C, "Trinity College London – İngilizce",    "Her Seviye", true, 9271),
            new(358, C, "Cambridge YLE – Çocuklar İçin İngilizce", "İlkokul", true, 9272),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  TEMEL EĞİTİM  root 9019
    //  İlkokul 9281 / Ortaokul 9282
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetTemelEgitim()
    {
        const string C = "Temel Eğitim";
        return
        [
            new(9019, C, "Temel Eğitim", "Her Seviye", true),
            new(9281, C, "İlkokul",      "İlkokul",    true, 9019),
            new(9282, C, "Ortaokul",     "Ortaokul",   true, 9019),
            new(700, C, "Genel Takviye",           "İlkokul", true, 9281),
            new(701, C, "Okuma Yazma",             "İlkokul", true, 9281),
            new(702, C, "İlkokul Matematik",      "İlkokul", true, 9281),
            new(703, C, "Okuma Yazmaya Hazırlık", "İlkokul", true, 9281),
            new(704, C, "Montessori Eğitimi",     "İlkokul", true, 9281),
            new(706, C, "Ödev Destek",            "İlkokul", true, 9281),
            new(708, C, "Okul Öncesi Eğitim",    "İlkokul", true, 9281),
            new(705, C, "Genel Takviye", "Ortaokul", true, 9282),
            new(707, C, "Ödev Destek",   "Ortaokul", true, 9282),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  ÖZEL EĞİTİM  root 9020 / Her Seviye 9291
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetOzelEgitim()
    {
        const string C = "Özel Eğitim";
        return
        [
            new(9020, C, "Özel Eğitim", "Her Seviye", true),
            new(9291, C, "Her Seviye",  "Her Seviye", true, 9020),
            new(720, C, "Otizm Spektrum Bozukluğu",         "Her Seviye", true, 9291),
            new(721, C, "Öğrenme Güçlüğü (Disleksi vb.)",   "Her Seviye", true, 9291),
            new(722, C, "DEHB – Dikkat Eksikliği",          "Her Seviye", true, 9291),
            new(723, C, "Zihinsel Yetersizlik",             "Her Seviye", true, 9291),
            new(724, C, "Görme / İşitme Yetersizliği",      "Her Seviye", true, 9291),
            new(725, C, "Dil ve Konuşma Bozuklukları",      "Her Seviye", true, 9291),
            new(726, C, "Üstün Zekalı Eğitim",              "Her Seviye", true, 9291),
            new(727, C, "Gölge Öğretmenlik",                "Her Seviye", true, 9291),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  ÜNİVERSİTE TAKVİYE  root 9021 / Üniversite 9301
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetUniversiteTakviye()
    {
        const string C = "Üniversite Dersleri Takviye";
        return
        [
            new(9021, C, "Üniversite Dersleri Takviye", "Üniversite", true),
            new(9301, C, "Üniversite", "Üniversite", true, 9021),
            new(740, C, "Mühendislik Matematiği",            "Üniversite", true, 9301),
            new(741, C, "İnşaat Mühendisliği Dersleri",      "Üniversite", true, 9301),
            new(742, C, "Elektrik-Elektronik Müh. Dersleri", "Üniversite", true, 9301),
            new(743, C, "Makine Mühendisliği Dersleri",      "Üniversite", true, 9301),
            new(744, C, "Tıp Fakültesi Dersleri",            "Üniversite", true, 9301),
            new(745, C, "Tez Yazımı Danışmanlığı",           "Üniversite", true, 9301),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  MÜZİK  root 9022 / Her Seviye 9311
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetMuzik()
    {
        const string C = "Müzik";
        return
        [
            new(9022, C, "Müzik",      "Her Seviye", true),
            new(9311, C, "Her Seviye", "Her Seviye", true, 9022),
            new(800, C, "Piyano",               "Her Seviye", true, 9311),
            new(801, C, "Gitar",                "Her Seviye", true, 9311),
            new(802, C, "Klasik Gitar",         "Her Seviye", true, 9311),
            new(803, C, "Keman",                "Her Seviye", true, 9311),
            new(804, C, "Bağlama",              "Her Seviye", true, 9311),
            new(805, C, "Flüt",                 "Her Seviye", true, 9311),
            new(806, C, "Saksafon",             "Her Seviye", true, 9311),
            new(807, C, "Klarnet",              "Her Seviye", true, 9311),
            new(808, C, "Bateri / Davul",       "Her Seviye", true, 9311),
            new(809, C, "Viyolonsel (Cello)",   "Her Seviye", true, 9311),
            new(810, C, "Ney",                  "Her Seviye", true, 9311),
            new(811, C, "Ud",                   "Her Seviye", true, 9311),
            new(812, C, "Kanun",                "Her Seviye", true, 9311),
            new(813, C, "Ukulele",              "Her Seviye", true, 9311),
            new(814, C, "Solfej",               "Her Seviye", true, 9311),
            new(815, C, "Müzik Teorisi",        "Her Seviye", true, 9311),
            new(816, C, "Ses Eğitimi (Vokal)",  "Her Seviye", true, 9311),
            new(817, C, "San (Opera / Vokal)",  "Her Seviye", true, 9311),
            new(818, C, "Jazz",                 "Her Seviye", true, 9311),
            new(819, C, "Makam Müziği",         "Her Seviye", true, 9311),
            new(820, C, "DJ Eğitimi",           "Her Seviye", true, 9311),
            new(821, C, "Armoni ve Kontrpuan",  "Her Seviye", true, 9311),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  SPOR  root 9023 / Her Seviye 9321
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetSpor()
    {
        const string C = "Spor";
        return
        [
            new(9023, C, "Spor",       "Her Seviye", true),
            new(9321, C, "Her Seviye", "Her Seviye", true, 9023),
            new(850, C, "Yüzme",           "Her Seviye", true, 9321),
            new(851, C, "Futbol",          "Her Seviye", true, 9321),
            new(852, C, "Basketbol",       "Her Seviye", true, 9321),
            new(853, C, "Voleybol",        "Her Seviye", true, 9321),
            new(854, C, "Tenis",           "Her Seviye", true, 9321),
            new(855, C, "Masa Tenisi",     "Her Seviye", true, 9321),
            new(856, C, "Badminton",       "Her Seviye", true, 9321),
            new(857, C, "Fitness",         "Her Seviye", true, 9321),
            new(858, C, "Yoga",            "Her Seviye", true, 9321),
            new(859, C, "Pilates",         "Her Seviye", true, 9321),
            new(860, C, "Zumba",           "Her Seviye", true, 9321),
            new(861, C, "Karate",          "Her Seviye", true, 9321),
            new(862, C, "Taekwondo",       "Her Seviye", true, 9321),
            new(863, C, "Judo",            "Her Seviye", true, 9321),
            new(864, C, "Boks",            "Her Seviye", true, 9321),
            new(865, C, "Kickboks",        "Her Seviye", true, 9321),
            new(866, C, "Muay Thai",       "Her Seviye", true, 9321),
            new(867, C, "Jiu-Jitsu",       "Her Seviye", true, 9321),
            new(868, C, "Jimnastik",       "Her Seviye", true, 9321),
            new(869, C, "Binicilik",       "Her Seviye", true, 9321),
            new(870, C, "Buz Pateni",      "Her Seviye", true, 9321),
            new(871, C, "Bisiklet",        "Her Seviye", true, 9321),
            new(872, C, "Okçuluk",         "Her Seviye", true, 9321),
            new(873, C, "Krav Maga",       "Her Seviye", true, 9321),
            new(874, C, "Personal Trainer","Her Seviye", true, 9321),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  DANS  root 9024 / Her Seviye 9331
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetDans()
    {
        const string C = "Dans";
        return
        [
            new(9024, C, "Dans",       "Her Seviye", true),
            new(9331, C, "Her Seviye",   "Her Seviye", true, 9024),
            new(900, C, "Bale",          "Her Seviye", true, 9331),
            new(901, C, "Halk Oyunları", "Her Seviye", true, 9331),
            new(902, C, "Modern Dans",   "Her Seviye", true, 9331),
            new(903, C, "Oryantal Dans", "Her Seviye", true, 9331),
            new(904, C, "Salsa",         "Her Seviye", true, 9331),
            new(905, C, "Bachata",       "Her Seviye", true, 9331),
            new(906, C, "Tango",         "Her Seviye", true, 9331),
            new(907, C, "Break Dans",    "Her Seviye", true, 9331),
            new(908, C, "Jazz Dans",     "Her Seviye", true, 9331),
            new(909, C, "Hip-Hop Dans",  "Her Seviye", true, 9331),
            new(910, C, "Vals",          "Her Seviye", true, 9331),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  SANAT  root 9025 / Her Seviye 9341
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetSanat()
    {
        const string C = "Sanat ve El Sanatları";
        return
        [
            new(9025, C, "Sanat ve El Sanatları", "Her Seviye", true),
            new(9341, C, "Her Seviye",           "Her Seviye", true, 9025),
            new(930, C, "Resim",                 "Her Seviye", true, 9341),
            new(931, C, "Yağlı Boya",            "Her Seviye", true, 9341),
            new(932, C, "Kara Kalem",            "Her Seviye", true, 9341),
            new(933, C, "Grafik Tasarım",        "Her Seviye", true, 9341),
            new(934, C, "Fotoğrafçılık",         "Her Seviye", true, 9341),
            new(935, C, "Seramik ve Çini",       "Her Seviye", true, 9341),
            new(936, C, "Ebru Sanatı",           "Her Seviye", true, 9341),
            new(937, C, "Kaligrafi",             "Her Seviye", true, 9341),
            new(938, C, "Takı Tasarımı",         "Her Seviye", true, 9341),
            new(939, C, "Dikiş ve Nakış",        "Her Seviye", true, 9341),
            new(940, C, "Moda Tasarımı",         "Her Seviye", true, 9341),
            new(941, C, "Oyunculuk ve Tiyatro",  "Her Seviye", true, 9341),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  BİLİŞİM TEKNOLOJİLERİ  root 9026
    //  Lise 9351 / Üniversite 9352 / Yetişkin 9353 / Her Seviye 9354
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetBilisim()
    {
        const string C = "Bilişim Teknolojileri";
        return
        [
            new(9026, C, "Bilişim Teknolojileri", "Her Seviye", true),
            new(9351, C, "Lise",                            "Lise",       true, 9026),
            new(9352, C, "Üniversite",                      "Üniversite", true, 9026),
            new(9353, C, "Yetişkin",                        "Yetişkin",   true, 9026),
            new(9354, C, "Her Seviye",                      "Her Seviye", true, 9026),
            new(1000, C, "Python",                                "Lise", true, 9351),
            new(1001, C, "Python",                          "Üniversite", true, 9352),
            new(1003, C, "Java",                            "Üniversite", true, 9352),
            new(1004, C, "C#",                              "Üniversite", true, 9352),
            new(1005, C, "C++",                             "Üniversite", true, 9352),
            new(1009, C, "Algoritma ve Veri Yapıları",      "Üniversite", true, 9352),
            new(1010, C, "Yapay Zeka ve Makine Öğrenmesi",  "Üniversite", true, 9352),
            new(1013, C, "MATLAB",                          "Üniversite", true, 9352),
            new(1015, C, "SolidWorks",                      "Üniversite", true, 9352),
            new(1002, C, "Python", "Yetişkin", true, 9353),
            new(1006, C, "PHP",                          "Her Seviye", true, 9354),
            new(1007, C, "SQL / Veritabanı",             "Her Seviye", true, 9354),
            new(1008, C, "HTML & CSS Web Geliştirme",    "Her Seviye", true, 9354),
            new(1011, C, "Mobil Uygulama Geliştirme",    "Her Seviye", true, 9354),
            new(1012, C, "Siber Güvenlik",               "Her Seviye", true, 9354),
            new(1014, C, "AutoCAD",                      "Her Seviye", true, 9354),
            new(1016, C, "Revit",                        "Her Seviye", true, 9354),
            new(1017, C, "Photoshop",                    "Her Seviye", true, 9354),
            new(1018, C, "Illustrator",                  "Her Seviye", true, 9354),
            new(1019, C, "Adobe Premiere Pro",           "Her Seviye", true, 9354),
            new(1020, C, "After Effects",                "Her Seviye", true, 9354),
            new(1021, C, "Blender 3D Modelleme",         "Her Seviye", true, 9354),
            new(1022, C, "Microsoft Excel",              "Her Seviye", true, 9354),
            new(1023, C, "Microsoft Office Programları", "Her Seviye", true, 9354),
            new(1024, C, "SEO",                          "Her Seviye", true, 9354),
            new(1025, C, "Dijital Pazarlama",            "Her Seviye", true, 9354),
            new(1026, C, "Unity – Oyun Geliştirme",      "Her Seviye", true, 9354),
            new(1027, C, "WordPress",                    "Her Seviye", true, 9354),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  ROBOTİK VE KODLAMA  root 9027
    //  İlkokul 9361 / Ortaokul 9362 / Her Seviye 9363
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetRobotik()
    {
        const string C = "Robotik ve Kodlama";
        return
        [
            new(9027, C, "Robotik ve Kodlama", "Her Seviye", true),
            new(9361, C, "İlkokul",    "İlkokul",    true, 9027),
            new(9362, C, "Ortaokul",   "Ortaokul",   true, 9027),
            new(9363, C, "Her Seviye", "Her Seviye", true, 9027),
            new(1062, C, "Scratch – Görsel Kodlama", "İlkokul",    true, 9361),
            new(1064, C, "Lego Robotik",             "İlkokul",    true, 9361),
            new(1063, C, "Scratch – Görsel Kodlama", "Ortaokul",   true, 9362),
            new(1065, C, "Lego Robotik",             "Ortaokul",   true, 9362),
            new(1060, C, "Robotik Kodlama (Genel)",  "Her Seviye", true, 9363),
            new(1061, C, "Arduino Programlama",      "Her Seviye", true, 9363),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  DANIŞMANLIK VE KOÇLUK  root 9028
    //  Yetişkin 9371 / Her Seviye 9372
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetDanismanlik()
    {
        const string C = "Danışmanlık ve Koçluk";
        return
        [
            new(9028, C, "Danışmanlık ve Koçluk", "Her Seviye", true),
            new(9371, C, "Yetişkin",   "Yetişkin",   true, 9028),
            new(9372, C, "Her Seviye", "Her Seviye", true, 9028),
            new(1101, C, "Kariyer Koçluğu",   "Yetişkin",   true, 9371),
            new(1102, C, "Yaşam Koçluğu",     "Yetişkin",   true, 9371),
            new(1103, C, "Aile Danışmanlığı", "Yetişkin",   true, 9371),
            new(1104, C, "NLP",               "Yetişkin",   true, 9371),
            new(1100, C, "Eğitim Koçluğu",    "Her Seviye", true, 9372),
            new(1105, C, "Etkili İletişim",   "Her Seviye", true, 9372),
            new(1106, C, "Sunum Teknikleri",  "Her Seviye", true, 9372),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  MUHASEBE VE FİNANS  root 9029
    //  Üniversite 9381 / Yetişkin 9382
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetMuhasebe()
    {
        const string C = "Muhasebe ve Finans";
        return
        [
            new(9029, C, "Muhasebe ve Finans", "Her Seviye", true),
            new(9381, C, "Üniversite", "Üniversite", true, 9029),
            new(9382, C, "Yetişkin",   "Yetişkin",   true, 9029),
            new(1130, C, "Genel Muhasebe",     "Üniversite", true, 9381),
            new(1132, C, "Maliyet Muhasebesi", "Üniversite", true, 9381),
            new(1133, C, "Finans",             "Üniversite", true, 9381),
            new(1134, C, "Denetim",            "Üniversite", true, 9381),
            new(1131, C, "Genel Muhasebe",  "Yetişkin", true, 9382),
            new(1135, C, "Vergi Hukuku",    "Yetişkin", true, 9382),
            new(1136, C, "Bütçe Yönetimi", "Yetişkin", true, 9382),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  KİŞİSEL GELİŞİM  root 9030 / Her Seviye 9391
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetKisiselGelisim()
    {
        const string C = "Kişisel Gelişim";
        return
        [
            new(9030, C, "Kişisel Gelişim", "Her Seviye", true),
            new(9391, C, "Her Seviye",       "Her Seviye", true, 9030),
            new(1150, C, "Zaman Yönetimi",              "Her Seviye", true, 9391),
            new(1151, C, "Stres Yönetimi",              "Her Seviye", true, 9391),
            new(1152, C, "Liderlik Becerileri",         "Her Seviye", true, 9391),
            new(1153, C, "Hızlı Okuma",                "Her Seviye", true, 9391),
            new(1154, C, "Bellek ve Hafıza Teknikleri", "Her Seviye", true, 9391),
            new(1155, C, "Meditasyon",                  "Her Seviye", true, 9391),
            new(1156, C, "İşaret Dili",                "Her Seviye", true, 9391),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  SAĞLIK VE YAŞAM  root 9031 / Her Seviye 9401
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetSaglik()
    {
        const string C = "Sağlık ve Yaşam";
        return
        [
            new(9031, C, "Sağlık ve Yaşam", "Her Seviye", true),
            new(9401, C, "Her Seviye",       "Her Seviye", true, 9031),
            new(1200, C, "Beslenme ve Diyetisyenlik",  "Her Seviye", true, 9401),
            new(1201, C, "İlk Yardım",                 "Her Seviye", true, 9401),
            new(1202, C, "Makyaj",                     "Her Seviye", true, 9401),
            new(1203, C, "Cilt Bakımı",                "Her Seviye", true, 9401),
            new(1204, C, "Kuaförlük ve Saç Bakımı",   "Her Seviye", true, 9401),
            new(1205, C, "Türk Mutfağı",               "Her Seviye", true, 9401),
            new(1206, C, "Pastacılık ve Fırıncılık",  "Her Seviye", true, 9401),
            new(1207, C, "Barista / Kahve Sanatı",     "Her Seviye", true, 9401),
            new(1208, C, "Tur Rehberliği",             "Her Seviye", true, 9401),
            new(1209, C, "Direksiyon Eğitimi",         "Her Seviye", true, 9401),
        ];
    }

    // ════════════════════════════════════════════════════════════════
    //  HOBİ VE DİĞER  root 9032
    //  Her Seviye 9411 / Lise 9412
    // ════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetHobi()
    {
        const string C = "Hobi ve Diğer";
        return
        [
            new(9032, C, "Hobi ve Diğer", "Her Seviye", true),
            new(1250, C, "Satranç",                           "Her Seviye", true, 9411),
            new(1251, C, "Rubik Küp",                         "Her Seviye", true, 9411),
            new(1255, C, "Müzik Yetenek Sınavı Hazırlığı",  "Lise", true, 9412),
            new(1256, C, "Güzel Sanatlar Sınavına Hazırlık","Lise", true, 9412),
        ];
    }
}