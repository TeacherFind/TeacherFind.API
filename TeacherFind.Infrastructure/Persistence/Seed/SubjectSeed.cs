using Microsoft.EntityFrameworkCore;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Persistence.Seed;

// ═══════════════════════════════════════════════════════════════════════════
//  SubjectSeed  –  TeacherFind API
//
//  STRUCTURE OVERVIEW
//  ──────────────────
//  Each row is a SubjectSeedItem(Code, Category, Name, Level, IsActive, ParentCode?)
//
//  Code ranges (do not overlap):
//    1–499      School Subjects
//    500–899    Yabancı Diller (Languages)
//    900–1299   Kodlama & Programlama
//    1300–1599  Müzik & Sanat
//    1600–1899  Spor & Dans
//    1900–2199  Diğer (Hobi, Yaratıcı, vb.)
//    9000–9999  Sınavlar (Türkiye + Uluslararası)
//
//  LEVEL RULES (applied exactly):
//    • Matematik, Fen Bilimleri, Coğrafya, Tarih, Felsefe,
//      Kimya, Fizik, Türk Edebiyatı  →  Genel + school-level children
//    • All other school subjects      →  Genel only (single entry)
//    • Languages                      →  Language chosen → level children (A1–C2)
//    • Kürtçe                         →  DELETED (not present)
//    • Sınavlar & Kodlama             →  Unchanged from previous version
// ═══════════════════════════════════════════════════════════════════════════

public static class SubjectSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var existingByCodes = await context.Subjects
            .Where(x => x.Code > 0)
            .GroupBy(x => x.Code)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.First());

        // Build a temporary code→Subject map so we can resolve ParentCode → ParentId
        var pendingByCode = new Dictionary<int, Subject>();

        foreach (var seed in GetSubjects())
        {
            var stage = DeriveStage(seed.Level);

            if (existingByCodes.TryGetValue(seed.Code, out var subject))
            {
                subject.Category = seed.Category;
                subject.Name = seed.Name;
                subject.Level = seed.Level;
                subject.Stage = stage;
                subject.IsActive = seed.IsActive;
                pendingByCode[seed.Code] = subject;
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
            pendingByCode[seed.Code] = subject;
        }

        // First save so all subjects get their Ids
        await context.SaveChangesAsync();

        // Second pass: wire ParentId
        foreach (var seed in GetSubjects().Where(s => s.ParentCode.HasValue))
        {
            if (!pendingByCode.TryGetValue(seed.Code, out var child)) continue;
            if (!pendingByCode.TryGetValue(seed.ParentCode!.Value, out var parent)) continue;
            child.ParentId = parent.Id;
        }

        await context.SaveChangesAsync();
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Stage derivation
    // ────────────────────────────────────────────────────────────────────────
    private static string DeriveStage(string level) => level switch
    {
        "İlkokul" => "İlköğretim",
        "Ortaokul" => "Ortaöğretim",
        "Lise" => "Lise",
        "Üniversite" => "Yükseköğretim",
        "Yetişkin" => "Yetişkin",
        "Her Seviye" => "Genel",
        _ => "Genel"
    };

    // ────────────────────────────────────────────────────────────────────────
    //  Seed item record
    // ────────────────────────────────────────────────────────────────────────
    private record SubjectSeedItem(
        int Code,
        string Category,
        string Name,
        string Level,
        bool IsActive,
        int? ParentCode = null);

    // ════════════════════════════════════════════════════════════════════════
    //  MASTER LIST
    // ════════════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetSubjects() =>
    [
        ..GetOkulDersleri(),
        ..GetYabanciDiller(),
        ..GetKodlama(),
        ..GetMuzikSanat(),
        ..GetSporDans(),
        ..GetDiger(),
        ..GetTurkiyeSinavlari(),
        ..GetUluslararasiSinavlar(),
    ];


    // ════════════════════════════════════════════════════════════════════════
    //  1.  OKUL DERSLERİ  (codes 1 – 499)
    //
    //  Rules:
    //    • Matematik, Fen Bilimleri, Coğrafya, Tarih, Felsefe, Kimya, Fizik,
    //      Türk Edebiyatı  → root "Genel" + children per school level
    //    • Everything else → single entry, Level = "Genel"
    // ════════════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetOkulDersleri()
    {
        const string C = "Okul Dersleri";
        return
        [
            // ── MATEMATİK ───────────────────────────────────────────────
            new(  1, C, "Matematik",           "Genel",    true),
            new(  2, C, "Matematik – İlkokul", "İlkokul",  true, ParentCode:  1),
            new(  3, C, "Matematik – Ortaokul","Ortaokul", true, ParentCode:  1),
            new(  4, C, "Matematik – Lise",    "Lise",     true, ParentCode:  1),

            // ── FEN BİLİMLERİ ───────────────────────────────────────────
            new( 10, C, "Fen Bilimleri",              "Genel",    true),
            new( 11, C, "Fen Bilimleri – İlkokul",   "İlkokul",  true, ParentCode: 10),
            new( 12, C, "Fen Bilimleri – Ortaokul",  "Ortaokul", true, ParentCode: 10),
            new( 13, C, "Fen Bilimleri – Lise",      "Lise",     true, ParentCode: 10),

            // ── COĞRAFİYA ───────────────────────────────────────────────
            // Note: taught as "Sosyal Bilgiler" in primary/middle, "Coğrafya" in high school
            new( 20, C, "Coğrafya",                        "Genel",    true),
            new( 21, C, "Coğrafya – Sosyal Bilgiler (Ortaokul)", "Ortaokul", true, ParentCode: 20),
            new( 22, C, "Coğrafya – Lise",                "Lise",     true, ParentCode: 20),

            // ── TARİH ───────────────────────────────────────────────────
            new( 30, C, "Tarih",              "Genel",    true),
            new( 31, C, "Tarih – Ortaokul",  "Ortaokul", true, ParentCode: 30),
            new( 32, C, "Tarih – Lise",      "Lise",     true, ParentCode: 30),

            // ── FELSEFE ─────────────────────────────────────────────────
            // Only taught at lise (10th–11th grade)
            new( 40, C, "Felsefe",           "Genel", true),
            new( 41, C, "Felsefe – Lise",   "Lise",  true, ParentCode: 40),

            // ── KİMYA ───────────────────────────────────────────────────
            // Only taught at lise (9th–12th grade)
            new( 50, C, "Kimya",             "Genel", true),
            new( 51, C, "Kimya – Lise",     "Lise",  true, ParentCode: 50),

            // ── FİZİK ───────────────────────────────────────────────────
            // Only taught at lise (9th–12th grade)
            new( 60, C, "Fizik",             "Genel", true),
            new( 61, C, "Fizik – Lise",     "Lise",  true, ParentCode: 60),

            // ── TÜRK EDEBİYATI / TÜRKÇE ─────────────────────────────────
            // "Türkçe" in ilkokul & ortaokul, "Türk Dili ve Edebiyatı" in lise
            new( 70, C, "Türk Edebiyatı / Türkçe",              "Genel",    true),
            new( 71, C, "Türkçe – İlkokul",                     "İlkokul",  true, ParentCode: 70),
            new( 72, C, "Türkçe – Ortaokul",                    "Ortaokul", true, ParentCode: 70),
            new( 73, C, "Türk Dili ve Edebiyatı – Lise",        "Lise",     true, ParentCode: 70),

            // ── BİYOLOJİ ────────────────────────────────────────────────
            // Genel only (lise subject but no separate school-level breakdown requested)
            new( 80, C, "Biyoloji",          "Genel", true),

            // ── HAYAT BİLGİSİ ───────────────────────────────────────────
            // Taught in grades 1–3 (İlkokul)
            new( 90, C, "Hayat Bilgisi",     "Genel", true),

            // ── SOSYAL BİLGİLER ─────────────────────────────────────────
            // Taught in grades 4–7 (İlkokul–Ortaokul), separate from Tarih/Coğrafya
            new(100, C, "Sosyal Bilgiler",   "Genel", true),

            // ── T.C. İNKILAP TARİHİ VE ATATÜRKÇÜLÜK ────────────────────
            // 8th grade ortaokul + 12th grade lise
            new(110, C, "T.C. İnkılap Tarihi ve Atatürkçülük", "Genel", true),

            // ── DİN KÜLTÜRÜ VE AHLAK BİLGİSİ ───────────────────────────
            new(120, C, "Din Kültürü ve Ahlak Bilgisi", "Genel", true),

            // ── PSİKOLOJİ ───────────────────────────────────────────────
            new(130, C, "Psikoloji",         "Genel", true),

            // ── SOSYOLOJİ ───────────────────────────────────────────────
            new(140, C, "Sosyoloji",         "Genel", true),

            // ── MANTIK ──────────────────────────────────────────────────
            new(150, C, "Mantık",            "Genel", true),

            // ── GÖRSEL SANATLAR ─────────────────────────────────────────
            new(160, C, "Görsel Sanatlar",   "Genel", true),

            // ── MÜZİK (okul dersi) ──────────────────────────────────────
            new(170, C, "Müzik (Ders)",      "Genel", true),

            // ── BEDEN EĞİTİMİ ───────────────────────────────────────────
            new(180, C, "Beden Eğitimi",     "Genel", true),

            // ── BİLİŞİM TEKNOLOJİLERİ ──────────────────────────────────
            // Taught from 5th grade onward
            new(190, C, "Bilişim Teknolojileri ve Yazılım", "Genel", true),

            // ── SAĞLIK BİLGİSİ ──────────────────────────────────────────
            new(200, C, "Sağlık Bilgisi",    "Genel", true),

            // ── TRAFİK GÜVENLİĞİ / İNSAN HAKLARI ───────────────────────
            new(210, C, "İnsan Hakları, Vatandaşlık ve Demokrasi", "Genel", true),

            // ── REHBERLİK ───────────────────────────────────────────────
            new(220, C, "Rehberlik ve Yönlendirme", "Genel", true),

            // ── MUHASEBE / EKONOMİ ──────────────────────────────────────
            new(230, C, "Ekonomi",           "Genel", true),
            new(231, C, "Muhasebe",          "Genel", true),

            // ── HUKUK ───────────────────────────────────────────────────
            new(240, C, "Hukuk",             "Genel", true),
        ];
    }


    // ════════════════════════════════════════════════════════════════════════
    //  2.  YABANCI DİLLER  (codes 500 – 899)
    //
    //  Rules:
    //    • Each language has a root node (Level = "Genel")
    //    • After the user selects a language, CEFR levels appear as children
    //    • Kürtçe → DELETED
    // ════════════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetYabanciDiller()
    {
        const string C = "Yabancı Diller";
        return
        [
            // ── İNGİLİZCE ───────────────────────────────────────────────
            new(500, C, "İngilizce",       "Genel",      true),
            new(501, C, "İngilizce – A1", "Her Seviye",  true, ParentCode: 500),
            new(502, C, "İngilizce – A2", "Her Seviye",  true, ParentCode: 500),
            new(503, C, "İngilizce – B1", "Her Seviye",  true, ParentCode: 500),
            new(504, C, "İngilizce – B2", "Her Seviye",  true, ParentCode: 500),
            new(505, C, "İngilizce – C1", "Her Seviye",  true, ParentCode: 500),
            new(506, C, "İngilizce – C2", "Her Seviye",  true, ParentCode: 500),

            // ── ALMANCA ─────────────────────────────────────────────────
            new(510, C, "Almanca",         "Genel",      true),
            new(511, C, "Almanca – A1",   "Her Seviye",  true, ParentCode: 510),
            new(512, C, "Almanca – A2",   "Her Seviye",  true, ParentCode: 510),
            new(513, C, "Almanca – B1",   "Her Seviye",  true, ParentCode: 510),
            new(514, C, "Almanca – B2",   "Her Seviye",  true, ParentCode: 510),
            new(515, C, "Almanca – C1",   "Her Seviye",  true, ParentCode: 510),
            new(516, C, "Almanca – C2",   "Her Seviye",  true, ParentCode: 510),

            // ── FRANSIZCA ───────────────────────────────────────────────
            new(520, C, "Fransızca",       "Genel",      true),
            new(521, C, "Fransızca – A1", "Her Seviye",  true, ParentCode: 520),
            new(522, C, "Fransızca – A2", "Her Seviye",  true, ParentCode: 520),
            new(523, C, "Fransızca – B1", "Her Seviye",  true, ParentCode: 520),
            new(524, C, "Fransızca – B2", "Her Seviye",  true, ParentCode: 520),
            new(525, C, "Fransızca – C1", "Her Seviye",  true, ParentCode: 520),
            new(526, C, "Fransızca – C2", "Her Seviye",  true, ParentCode: 520),

            // ── İSPANYOLCA ──────────────────────────────────────────────
            new(530, C, "İspanyolca",       "Genel",     true),
            new(531, C, "İspanyolca – A1", "Her Seviye", true, ParentCode: 530),
            new(532, C, "İspanyolca – A2", "Her Seviye", true, ParentCode: 530),
            new(533, C, "İspanyolca – B1", "Her Seviye", true, ParentCode: 530),
            new(534, C, "İspanyolca – B2", "Her Seviye", true, ParentCode: 530),
            new(535, C, "İspanyolca – C1", "Her Seviye", true, ParentCode: 530),
            new(536, C, "İspanyolca – C2", "Her Seviye", true, ParentCode: 530),

            // ── İTALYANCA ───────────────────────────────────────────────
            new(540, C, "İtalyanca",       "Genel",      true),
            new(541, C, "İtalyanca – A1", "Her Seviye",  true, ParentCode: 540),
            new(542, C, "İtalyanca – A2", "Her Seviye",  true, ParentCode: 540),
            new(543, C, "İtalyanca – B1", "Her Seviye",  true, ParentCode: 540),
            new(544, C, "İtalyanca – B2", "Her Seviye",  true, ParentCode: 540),
            new(545, C, "İtalyanca – C1", "Her Seviye",  true, ParentCode: 540),
            new(546, C, "İtalyanca – C2", "Her Seviye",  true, ParentCode: 540),

            // ── RUSÇA ───────────────────────────────────────────────────
            new(550, C, "Rusça",           "Genel",      true),
            new(551, C, "Rusça – A1",     "Her Seviye",  true, ParentCode: 550),
            new(552, C, "Rusça – A2",     "Her Seviye",  true, ParentCode: 550),
            new(553, C, "Rusça – B1",     "Her Seviye",  true, ParentCode: 550),
            new(554, C, "Rusça – B2",     "Her Seviye",  true, ParentCode: 550),
            new(555, C, "Rusça – C1",     "Her Seviye",  true, ParentCode: 550),
            new(556, C, "Rusça – C2",     "Her Seviye",  true, ParentCode: 550),

            // ── ARAPÇA ──────────────────────────────────────────────────
            new(560, C, "Arapça",          "Genel",      true),
            new(561, C, "Arapça – A1",    "Her Seviye",  true, ParentCode: 560),
            new(562, C, "Arapça – A2",    "Her Seviye",  true, ParentCode: 560),
            new(563, C, "Arapça – B1",    "Her Seviye",  true, ParentCode: 560),
            new(564, C, "Arapça – B2",    "Her Seviye",  true, ParentCode: 560),
            new(565, C, "Arapça – C1",    "Her Seviye",  true, ParentCode: 560),
            new(566, C, "Arapça – C2",    "Her Seviye",  true, ParentCode: 560),

            // ── ÇINCE (MANDARIN) ────────────────────────────────────────
            new(570, C, "Çince",           "Genel",      true),
            new(571, C, "Çince – HSK 1",  "Her Seviye",  true, ParentCode: 570),
            new(572, C, "Çince – HSK 2",  "Her Seviye",  true, ParentCode: 570),
            new(573, C, "Çince – HSK 3",  "Her Seviye",  true, ParentCode: 570),
            new(574, C, "Çince – HSK 4",  "Her Seviye",  true, ParentCode: 570),
            new(575, C, "Çince – HSK 5",  "Her Seviye",  true, ParentCode: 570),
            new(576, C, "Çince – HSK 6",  "Her Seviye",  true, ParentCode: 570),

            // ── JAPONCA ─────────────────────────────────────────────────
            new(580, C, "Japonca",          "Genel",     true),
            new(581, C, "Japonca – N5",    "Her Seviye", true, ParentCode: 580),
            new(582, C, "Japonca – N4",    "Her Seviye", true, ParentCode: 580),
            new(583, C, "Japonca – N3",    "Her Seviye", true, ParentCode: 580),
            new(584, C, "Japonca – N2",    "Her Seviye", true, ParentCode: 580),
            new(585, C, "Japonca – N1",    "Her Seviye", true, ParentCode: 580),

            // ── KORECE ──────────────────────────────────────────────────
            new(590, C, "Korece",           "Genel",     true),
            new(591, C, "Korece – TOPIK I (1-2)",   "Her Seviye", true, ParentCode: 590),
            new(592, C, "Korece – TOPIK II (3-4)",  "Her Seviye", true, ParentCode: 590),
            new(593, C, "Korece – TOPIK II (5-6)",  "Her Seviye", true, ParentCode: 590),

            // ── PORTEKIZCE ──────────────────────────────────────────────
            new(600, C, "Portekizce",       "Genel",     true),
            new(601, C, "Portekizce – A1", "Her Seviye", true, ParentCode: 600),
            new(602, C, "Portekizce – A2", "Her Seviye", true, ParentCode: 600),
            new(603, C, "Portekizce – B1", "Her Seviye", true, ParentCode: 600),
            new(604, C, "Portekizce – B2", "Her Seviye", true, ParentCode: 600),
            new(605, C, "Portekizce – C1", "Her Seviye", true, ParentCode: 600),
            new(606, C, "Portekizce – C2", "Her Seviye", true, ParentCode: 600),

            // ── FARSÇA ──────────────────────────────────────────────────
            new(610, C, "Farsça",           "Genel",     true),
            new(611, C, "Farsça – A1",    "Her Seviye",  true, ParentCode: 610),
            new(612, C, "Farsça – A2",    "Her Seviye",  true, ParentCode: 610),
            new(613, C, "Farsça – B1",    "Her Seviye",  true, ParentCode: 610),
            new(614, C, "Farsça – B2",    "Her Seviye",  true, ParentCode: 610),
            new(615, C, "Farsça – C1",    "Her Seviye",  true, ParentCode: 610),
            new(616, C, "Farsça – C2",    "Her Seviye",  true, ParentCode: 610),

            // ── TÜRKÇE (Yabancılar İçin) ────────────────────────────────
            new(620, C, "Türkçe (Yabancılar İçin)", "Genel", true),
            new(621, C, "Türkçe (Yabancılar İçin) – A1", "Her Seviye", true, ParentCode: 620),
            new(622, C, "Türkçe (Yabancılar İçin) – A2", "Her Seviye", true, ParentCode: 620),
            new(623, C, "Türkçe (Yabancılar İçin) – B1", "Her Seviye", true, ParentCode: 620),
            new(624, C, "Türkçe (Yabancılar İçin) – B2", "Her Seviye", true, ParentCode: 620),
            new(625, C, "Türkçe (Yabancılar İçin) – C1", "Her Seviye", true, ParentCode: 620),
            new(626, C, "Türkçe (Yabancılar İçin) – C2", "Her Seviye", true, ParentCode: 620),

            // ── LATINCE ─────────────────────────────────────────────────
            new(630, C, "Latince",          "Genel",     true),
            new(631, C, "Latince – Başlangıç",  "Her Seviye", true, ParentCode: 630),
            new(632, C, "Latince – Orta",       "Her Seviye", true, ParentCode: 630),
            new(633, C, "Latince – İleri",      "Her Seviye", true, ParentCode: 630),

            // ── OSMANLICA ───────────────────────────────────────────────
            new(640, C, "Osmanlıca",        "Genel",     true),
            new(641, C, "Osmanlıca – Başlangıç", "Her Seviye", true, ParentCode: 640),
            new(642, C, "Osmanlıca – Orta",      "Her Seviye", true, ParentCode: 640),
            new(643, C, "Osmanlıca – İleri",     "Her Seviye", true, ParentCode: 640),
        ];
    }


    // ════════════════════════════════════════════════════════════════════════
    //  3.  KODLAMA & PROGRAMLAMA  (codes 900 – 1299)
    // ════════════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetKodlama()
    {
        const string C = "Kodlama & Programlama";
        return
        [
            new(900, C, "Kodlama & Programlama", "Her Seviye", true),

            // Temel / Çocuklar
            new(901, C, "Scratch (Blok Kodlama)",   "Her Seviye", true, ParentCode: 900),
            new(902, C, "HTML & CSS",                "Her Seviye", true, ParentCode: 900),

            // Backend
            new(910, C, "Python",       "Her Seviye", true, ParentCode: 900),
            new(911, C, "Java",         "Her Seviye", true, ParentCode: 900),
            new(912, C, "C#",           "Her Seviye", true, ParentCode: 900),
            new(913, C, "C / C++",      "Her Seviye", true, ParentCode: 900),
            new(914, C, "PHP",          "Her Seviye", true, ParentCode: 900),
            new(915, C, "Go",           "Her Seviye", true, ParentCode: 900),
            new(916, C, "Rust",         "Her Seviye", true, ParentCode: 900),
            new(917, C, "Ruby",         "Her Seviye", true, ParentCode: 900),

            // Frontend
            new(920, C, "JavaScript",   "Her Seviye", true, ParentCode: 900),
            new(921, C, "TypeScript",   "Her Seviye", true, ParentCode: 900),
            new(922, C, "React",        "Her Seviye", true, ParentCode: 900),
            new(923, C, "Vue.js",       "Her Seviye", true, ParentCode: 900),
            new(924, C, "Angular",      "Her Seviye", true, ParentCode: 900),

            // Mobil
            new(930, C, "Swift (iOS)",          "Her Seviye", true, ParentCode: 900),
            new(931, C, "Kotlin (Android)",     "Her Seviye", true, ParentCode: 900),
            new(932, C, "Flutter / Dart",       "Her Seviye", true, ParentCode: 900),
            new(933, C, "React Native",         "Her Seviye", true, ParentCode: 900),

            // Veri Bilimi / AI
            new(940, C, "Python – Veri Bilimi", "Her Seviye", true, ParentCode: 900),
            new(941, C, "R",                    "Her Seviye", true, ParentCode: 900),
            new(942, C, "Machine Learning",     "Her Seviye", true, ParentCode: 900),
            new(943, C, "Yapay Zeka (AI)",      "Her Seviye", true, ParentCode: 900),

            // Veritabanı
            new(950, C, "SQL",          "Her Seviye", true, ParentCode: 900),
            new(951, C, "PostgreSQL",   "Her Seviye", true, ParentCode: 900),
            new(952, C, "MongoDB",      "Her Seviye", true, ParentCode: 900),
            new(953, C, "Firebase",     "Her Seviye", true, ParentCode: 900),

            // DevOps / Araçlar
            new(960, C, "Git & GitHub", "Her Seviye", true, ParentCode: 900),
            new(961, C, "Docker",       "Her Seviye", true, ParentCode: 900),
            new(962, C, "Linux / Bash", "Her Seviye", true, ParentCode: 900),

            // Oyun Geliştirme
            new(970, C, "Unity (C#)",           "Her Seviye", true, ParentCode: 900),
            new(971, C, "Unreal Engine",        "Her Seviye", true, ParentCode: 900),
            new(972, C, "Godot",                "Her Seviye", true, ParentCode: 900),
        ];
    }


    // ════════════════════════════════════════════════════════════════════════
    //  4.  MÜZİK & SANAT  (codes 1300 – 1599)
    // ════════════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetMuzikSanat()
    {
        const string C = "Müzik & Sanat";
        return
        [
            new(1300, C, "Müzik & Sanat", "Her Seviye", true),

            // Enstrümanlar
            new(1301, C, "Piyano",          "Her Seviye", true, ParentCode: 1300),
            new(1302, C, "Gitar",           "Her Seviye", true, ParentCode: 1300),
            new(1303, C, "Keman",           "Her Seviye", true, ParentCode: 1300),
            new(1304, C, "Bağlama / Saz",  "Her Seviye", true, ParentCode: 1300),
            new(1305, C, "Ud",              "Her Seviye", true, ParentCode: 1300),
            new(1306, C, "Ney",             "Her Seviye", true, ParentCode: 1300),
            new(1307, C, "Davul & Perküsyon","Her Seviye",true, ParentCode: 1300),
            new(1308, C, "Flüt",            "Her Seviye", true, ParentCode: 1300),
            new(1309, C, "Keman – Klasik",  "Her Seviye", true, ParentCode: 1300),

            // Ses / Şan
            new(1320, C, "Ses Eğitimi / Şan", "Her Seviye", true, ParentCode: 1300),

            // Görsel Sanatlar
            new(1330, C, "Resim & Çizim",     "Her Seviye", true, ParentCode: 1300),
            new(1331, C, "Heykel",             "Her Seviye", true, ParentCode: 1300),
            new(1332, C, "Ebru Sanatı",        "Her Seviye", true, ParentCode: 1300),
            new(1333, C, "Hat Sanatı",         "Her Seviye", true, ParentCode: 1300),
            new(1334, C, "Dijital Tasarım",    "Her Seviye", true, ParentCode: 1300),
            new(1335, C, "Fotoğrafçılık",      "Her Seviye", true, ParentCode: 1300),
        ];
    }


    // ════════════════════════════════════════════════════════════════════════
    //  5.  SPOR & DANS  (codes 1600 – 1899)
    // ════════════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetSporDans()
    {
        const string C = "Spor & Dans";
        return
        [
            new(1600, C, "Spor & Dans", "Her Seviye", true),

            // Spor
            new(1601, C, "Futbol",          "Her Seviye", true, ParentCode: 1600),
            new(1602, C, "Basketbol",       "Her Seviye", true, ParentCode: 1600),
            new(1603, C, "Voleybol",        "Her Seviye", true, ParentCode: 1600),
            new(1604, C, "Tenis",           "Her Seviye", true, ParentCode: 1600),
            new(1605, C, "Yüzme",           "Her Seviye", true, ParentCode: 1600),
            new(1606, C, "Jimnastik",       "Her Seviye", true, ParentCode: 1600),
            new(1607, C, "Güreş",           "Her Seviye", true, ParentCode: 1600),
            new(1608, C, "Karate / Judo",   "Her Seviye", true, ParentCode: 1600),
            new(1609, C, "Satranç",         "Her Seviye", true, ParentCode: 1600),

            // Dans
            new(1620, C, "Halk Dansları",       "Her Seviye", true, ParentCode: 1600),
            new(1621, C, "Bale",                "Her Seviye", true, ParentCode: 1600),
            new(1622, C, "Modern Dans",         "Her Seviye", true, ParentCode: 1600),
            new(1623, C, "Latin Dansları",      "Her Seviye", true, ParentCode: 1600),
        ];
    }


    // ════════════════════════════════════════════════════════════════════════
    //  6.  DİĞER  (codes 1900 – 2199)
    // ════════════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetDiger()
    {
        const string C = "Diğer";
        return
        [
            new(1900, C, "Diğer",               "Her Seviye", true),
            new(1901, C, "Girişimcilik",        "Her Seviye", true, ParentCode: 1900),
            new(1902, C, "Yazarlık / Yaratıcı Yazarlık", "Her Seviye", true, ParentCode: 1900),
            new(1903, C, "Sunum & Konuşma Teknikleri",  "Her Seviye", true, ParentCode: 1900),
            new(1904, C, "Zeka Oyunları",        "Her Seviye", true, ParentCode: 1900),
        ];
    }


    // ════════════════════════════════════════════════════════════════════════
    //  7.  TÜRKİYE SINAVLARI  (codes 9000 – 9499)
    // ════════════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetTurkiyeSinavlari()
    {
        const string C = "Türkiye Sınavları";
        return
        [
            // ── Root ────────────────────────────────────────────────────
            new(9000, C, "Türkiye Sınavları", "Her Seviye", true),

            // ── Level nodes ─────────────────────────────────────────────
            new(9001, C, "Ortaokul",   "Ortaokul",   true, ParentCode: 9000),
            new(9002, C, "Lise",       "Lise",        true, ParentCode: 9000),
            new(9003, C, "Üniversite", "Üniversite",  true, ParentCode: 9000),
            new(9004, C, "Yetişkin",   "Yetişkin",    true, ParentCode: 9000),

            // ── ORTAOKUL ─────────────────────────────────────────────────
            new(9010, C, "LGS – Liselere Geçiş Sınavı",  "Ortaokul", true, ParentCode: 9001),
            new(9011, C, "LGS – Genel Hazırlık",          "Ortaokul", true, ParentCode: 9010),
            new(9012, C, "LGS – Matematik",               "Ortaokul", true, ParentCode: 9010),
            new(9013, C, "LGS – Türkçe",                  "Ortaokul", true, ParentCode: 9010),
            new(9014, C, "LGS – Fen Bilimleri",           "Ortaokul", true, ParentCode: 9010),
            new(9015, C, "LGS – İnkılap Tarihi ve Din Kültürü", "Ortaokul", true, ParentCode: 9010),
            new(9016, C, "İOKBS – Bursluluk Sınavı",      "Ortaokul", true, ParentCode: 9001),

            // ── LİSE ─────────────────────────────────────────────────────
            new(9020, C, "YKS – Yükseköğretim Kurumları Sınavı", "Lise", true, ParentCode: 9002),
            new(9021, C, "YKS – Genel Hazırlık (TYT + AYT)",    "Lise", true, ParentCode: 9020),
            new(9022, C, "TYT – Temel Yeterlilik Testi",         "Lise", true, ParentCode: 9020),
            new(9023, C, "AYT – Sayısal",                        "Lise", true, ParentCode: 9020),
            new(9024, C, "AYT – Eşit Ağırlık",                  "Lise", true, ParentCode: 9020),
            new(9025, C, "AYT – Sözel",                          "Lise", true, ParentCode: 9020),
            new(9026, C, "YDT – Yabancı Dil Testi",             "Lise", true, ParentCode: 9020),
            new(9027, C, "MSÜ – Milli Savunma Üniversitesi",    "Lise", true, ParentCode: 9002),
            new(9028, C, "POMEM – Polis Okulu",                  "Lise", true, ParentCode: 9002),
            new(9029, C, "PAEM – Jandarma Okulu",                "Lise", true, ParentCode: 9002),

            // ── ÜNİVERSİTE / YETİŞKİN ────────────────────────────────────
            new(9030, C, "KPSS – Kamu Personeli Seçme Sınavı",  "Yetişkin", true, ParentCode: 9004),
            new(9031, C, "KPSS – Genel Yetenek & Genel Kültür", "Yetişkin", true, ParentCode: 9030),
            new(9032, C, "KPSS – Eğitim Bilimleri",             "Yetişkin", true, ParentCode: 9030),
            new(9033, C, "KPSS – Alan Bilgisi",                  "Yetişkin", true, ParentCode: 9030),

            new(9040, C, "ALES – Akademik Personel ve Lisansüstü Sınavı", "Üniversite", true, ParentCode: 9003),
            new(9041, C, "ALES – Sayısal",   "Üniversite", true, ParentCode: 9040),
            new(9042, C, "ALES – Sözel",     "Üniversite", true, ParentCode: 9040),
            new(9043, C, "ALES – Eşit Ağırlık", "Üniversite", true, ParentCode: 9040),

            new(9050, C, "DGS – Dikey Geçiş Sınavı",            "Üniversite", true, ParentCode: 9003),
            new(9051, C, "EKPSS – Engelli Kamu Personeli Sınavı","Yetişkin",  true, ParentCode: 9004),
            new(9052, C, "TUS – Tıpta Uzmanlık Sınavı",         "Yetişkin",  true, ParentCode: 9004),
            new(9053, C, "DUS – Diş Hekimliği Uzmanlık Sınavı", "Yetişkin",  true, ParentCode: 9004),

            // ── YDS / YÖKDİL / e-YDS (dil seçimiyle) ────────────────────
            new(9060, C, "YDS – Yabancı Dil Sınavı",            "Yetişkin", true, ParentCode: 9004),
            new(9061, C, "YDS – İngilizce",  "Yetişkin", true, ParentCode: 9060),
            new(9062, C, "YDS – Almanca",    "Yetişkin", true, ParentCode: 9060),
            new(9063, C, "YDS – Fransızca",  "Yetişkin", true, ParentCode: 9060),
            new(9064, C, "YDS – Arapça",     "Yetişkin", true, ParentCode: 9060),
            new(9065, C, "YDS – Rusça",      "Yetişkin", true, ParentCode: 9060),
            new(9066, C, "YDS – İspanyolca", "Yetişkin", true, ParentCode: 9060),

            new(9070, C, "YÖKDİL",               "Yetişkin", true, ParentCode: 9004),
            new(9071, C, "YÖKDİL – İngilizce",  "Yetişkin", true, ParentCode: 9070),
            new(9072, C, "YÖKDİL – Almanca",    "Yetişkin", true, ParentCode: 9070),
            new(9073, C, "YÖKDİL – Fransızca",  "Yetişkin", true, ParentCode: 9070),
            new(9074, C, "YÖKDİL – Arapça",     "Yetişkin", true, ParentCode: 9070),

            new(9080, C, "e-YDS",                "Yetişkin", true, ParentCode: 9004),
        ];
    }


    // ════════════════════════════════════════════════════════════════════════
    //  8.  ULUSLARARASI SINAVLAR  (codes 9500 – 9799)
    // ════════════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetUluslararasiSinavlar()
    {
        const string C = "Uluslararası Sınavlar";
        return
        [
            new(9500, C, "Uluslararası Sınavlar", "Her Seviye", true),

            // İngilizce Dil Sınavları
            new(9501, C, "IELTS",              "Her Seviye", true, ParentCode: 9500),
            new(9502, C, "TOEFL iBT",          "Her Seviye", true, ParentCode: 9500),
            new(9503, C, "Cambridge (FCE / CAE / CPE)", "Her Seviye", true, ParentCode: 9500),
            new(9504, C, "Duolingo English Test",       "Her Seviye", true, ParentCode: 9500),
            new(9505, C, "PTE Academic",       "Her Seviye", true, ParentCode: 9500),

            // Almanca Sertifika Sınavları
            new(9510, C, "Goethe-Zertifikat (A1–C2)", "Her Seviye", true, ParentCode: 9500),
            new(9511, C, "TestDaF",                    "Her Seviye", true, ParentCode: 9500),
            new(9512, C, "DSH (Deutsche Sprachprüfung)","Her Seviye",true, ParentCode: 9500),

            // Fransızca Sertifika Sınavları
            new(9520, C, "DELF (A1–B2)",  "Her Seviye", true, ParentCode: 9500),
            new(9521, C, "DALF (C1–C2)",  "Her Seviye", true, ParentCode: 9500),
            new(9522, C, "TCF",            "Her Seviye", true, ParentCode: 9500),
            new(9523, C, "TEF",            "Her Seviye", true, ParentCode: 9500),

            // İspanyolca Sertifika Sınavları
            new(9530, C, "DELE",   "Her Seviye", true, ParentCode: 9500),
            new(9531, C, "SIELE",  "Her Seviye", true, ParentCode: 9500),

            // İtalyanca
            new(9540, C, "CELI / CILS (İtalyanca Sertifika)", "Her Seviye", true, ParentCode: 9500),

            // Üniversite Giriş (ABD)
            new(9550, C, "SAT",   "Lise", true, ParentCode: 9500),
            new(9551, C, "ACT",   "Lise", true, ParentCode: 9500),
            new(9552, C, "AP (Advanced Placement)", "Lise", true, ParentCode: 9500),
            new(9553, C, "IB – International Baccalaureate", "Lise", true, ParentCode: 9500),

            // Lisansüstü (ABD)
            new(9560, C, "GRE",  "Üniversite", true, ParentCode: 9500),
            new(9561, C, "GMAT", "Üniversite", true, ParentCode: 9500),
            new(9562, C, "LSAT", "Üniversite", true, ParentCode: 9500),
            new(9563, C, "MCAT", "Üniversite", true, ParentCode: 9500),
        ];
    }
}