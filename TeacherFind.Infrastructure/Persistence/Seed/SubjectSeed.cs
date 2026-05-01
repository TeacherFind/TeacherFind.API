using Microsoft.EntityFrameworkCore;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Seeds;

/// <summary>
/// Ders (Subject) seed dosyası.
///
/// DepartmentSeed ile aynı pattern kullanılır:
///   new(Code, Category, Name, Level, IsActive)
///
/// Seviyeler:
///   "İlkokul"    → 1.–4. sınıf
///   "Ortaokul"   → 5.–8. sınıf
///   "Lise"       → 9.–12. sınıf
///   "Üniversite" → lisans / lisansüstü
///   "Yetişkin"   → mezun, kariyer, mesleki
///   "Her Seviye" → tüm öğrenci grupları (dil, müzik, spor vb.)
///
/// Eşleştirme Code üzerinden yapılır → SubjectSeed önce çalışmalıdır.
/// </summary>
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
            if (existingByCodes.TryGetValue(seed.Code, out var subject))
            {
                subject.Category = seed.Category;
                subject.Name = seed.Name;
                subject.Level = seed.Level;
                subject.IsActive = seed.IsActive;
            }
            else
            {
                subject = new Subject
                {
                    Code = seed.Code,
                    Category = seed.Category,
                    Name = seed.Name,
                    Level = seed.Level,
                    IsActive = seed.IsActive,
                };
                context.Subjects.Add(subject);
                existingByCodes[seed.Code] = subject;
            }
        }

        await context.SaveChangesAsync();
    }

    // ───────────────────────────────────────────────────────────────────
    //  MASTER LIST
    // ───────────────────────────────────────────────────────────────────
    private static List<SubjectSeedItem> GetSubjects()
    {
        var list = new List<SubjectSeedItem>();
        list.AddRange(GetMatematik());
        list.AddRange(GetTurkiyeSinavlari());
        list.AddRange(GetUluslararasiSinavlar());
        list.AddRange(GetAlmancaSinavlari());
        list.AddRange(GetDigerDilSertifikalari());
        list.AddRange(GetYabanciDiller());
        list.AddRange(GetFenBilimleri());
        list.AddRange(GetFizik());
        list.AddRange(GetKimya());
        list.AddRange(GetBiyoloji());
        list.AddRange(GetTurkceEdebiyat());
        list.AddRange(GetSosyalTarihCografya());
        list.AddRange(GetFelsefePsikoloji());
        list.AddRange(GetIstatistikEkonomiHukuk());
        list.AddRange(GetTemelEgitim());
        list.AddRange(GetOzelEgitim());
        list.AddRange(GetUniversiteTakviye());
        list.AddRange(GetMuzik());
        list.AddRange(GetSpor());
        list.AddRange(GetDans());
        list.AddRange(GetSanat());
        list.AddRange(GetBilisimTeknolojileri());
        list.AddRange(GetRobotikKodlama());
        list.AddRange(GetDanismanlikKocluk());
        list.AddRange(GetMuhasebeFinans());
        list.AddRange(GetKisiselGelisim());
        list.AddRange(GetSaglikYasam());
        list.AddRange(GetHobiDiger());
        return list;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  MATEMATİK  (Codes 1 – 40)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetMatematik()
    {
        const string C = "Matematik";
        return
        [
            // İlkokul
            new( 1, C, "Genel Matematik",           "İlkokul",    true),
            new( 2, C, "Mental Aritmetik",           "İlkokul",    true),
            new( 3, C, "Sayılar ve Dört İşlem",     "İlkokul",    true),

            // Ortaokul
            new( 4, C, "Genel Matematik",           "Ortaokul",   true),
            new( 5, C, "Geometri",                  "Ortaokul",   true),
            new( 6, C, "Olasılık ve İstatistik",   "Ortaokul",   true),
            new( 7, C, "Cebir",                     "Ortaokul",   true),

            // Lise
            new( 8, C, "Genel Matematik",           "Lise",       true),
            new( 9, C, "Geometri",                  "Lise",       true),
            new(10, C, "Analitik Geometri",         "Lise",       true),
            new(11, C, "Trigonometri",              "Lise",       true),
            new(12, C, "Olasılık ve İstatistik",   "Lise",       true),
            new(13, C, "Calculus (Temel)",          "Lise",       true),
            new(14, C, "Sayılar Teorisi",           "Lise",       true),

            // Üniversite
            new(15, C, "Calculus / Analiz",         "Üniversite", true),
            new(16, C, "Diferansiyel Denklemler",   "Üniversite", true),
            new(17, C, "Lineer Cebir",              "Üniversite", true),
            new(18, C, "Matematiksel Modelleme",    "Üniversite", true),
            new(19, C, "Ayrık Matematik",           "Üniversite", true),
            new(20, C, "Yöneylem Araştırması",      "Üniversite", true),
            new(21, C, "İleri Olasılık Teorisi",   "Üniversite", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  TÜRKİYE SINAVLARI  (Codes 100 – 199)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetTurkiyeSinavlari()
    {
        const string C = "Türkiye Sınavları";
        return
        [
            // Ortaokul sınavları
            new(100, C, "LGS – Genel Hazırlık",                    "Ortaokul",   true),
            new(101, C, "LGS – Matematik",                          "Ortaokul",   true),
            new(102, C, "LGS – Türkçe",                             "Ortaokul",   true),
            new(103, C, "LGS – Fen Bilimleri",                     "Ortaokul",   true),
            new(104, C, "LGS – İnkılap Tarihi ve Din Kültürü",    "Ortaokul",   true),
            new(105, C, "İOKBS – Bursluluk Sınavı",               "Ortaokul",   true),

            // Lise sınavları (YKS)
            new(110, C, "YKS – Genel Hazırlık (TYT + AYT)",       "Lise",       true),
            new(111, C, "TYT – Temel Yeterlilik Testi",            "Lise",       true),
            new(112, C, "AYT – Sayısal",                            "Lise",       true),
            new(113, C, "AYT – Eşit Ağırlık",                     "Lise",       true),
            new(114, C, "AYT – Sözel",                             "Lise",       true),
            new(115, C, "YDT – Yabancı Dil Testi",                "Lise",       true),
            new(116, C, "MSÜ – Milli Savunma Üniversitesi",       "Lise",       true),
            new(117, C, "Polis Akademisi Giriş Sınavı (PAFS)",     "Lise",       true),

            // YDS – her dil ayrı (Üniversite)
            new(120, C, "YDS – İngilizce",                          "Üniversite", true),
            new(121, C, "YDS – Almanca",                            "Üniversite", true),
            new(122, C, "YDS – Fransızca",                          "Üniversite", true),
            new(123, C, "YDS – Arapça",                             "Üniversite", true),
            new(124, C, "YDS – Rusça",                              "Üniversite", true),
            new(125, C, "YDS – İspanyolca",                         "Üniversite", true),
            new(126, C, "YDS – İtalyanca",                          "Üniversite", true),
            new(127, C, "YDS – Japonca",                            "Üniversite", true),
            new(128, C, "YDS – Çince",                              "Üniversite", true),
            new(129, C, "YDS – Portekizce",                         "Üniversite", true),

            // YÖKDİL – her dil ayrı (Üniversite)
            new(130, C, "YÖKDİL – İngilizce",                       "Üniversite", true),
            new(131, C, "YÖKDİL – Almanca",                         "Üniversite", true),
            new(132, C, "YÖKDİL – Fransızca",                       "Üniversite", true),
            new(133, C, "YÖKDİL – Arapça",                          "Üniversite", true),
            new(134, C, "YÖKDİL – Rusça",                           "Üniversite", true),
            new(135, C, "YÖKDİL – İspanyolca",                      "Üniversite", true),
            new(136, C, "YÖKDİL – İtalyanca",                       "Üniversite", true),
            new(137, C, "YÖKDİL – Japonca",                         "Üniversite", true),
            new(138, C, "YÖKDİL – Çince",                           "Üniversite", true),
            new(139, C, "YÖKDİL – Portekizce",                      "Üniversite", true),

            // Yüksek Lisans & Akademik (Üniversite / Yetişkin)
            new(140, C, "ALES – Akademik Personel ve Lisansüstü",   "Üniversite", true),
            new(141, C, "DGS – Dikey Geçiş Sınavı",                "Üniversite", true),
            new(142, C, "TUS – Tıpta Uzmanlık Sınavı",             "Yetişkin",   true),
            new(143, C, "DUS – Diş Hekimliği Uzmanlık Sınavı",     "Yetişkin",   true),
            new(144, C, "Eczacılık Uzmanlık Sınavı",                "Yetişkin",   true),
            new(145, C, "AÖF – Açıköğretim Sınavları",             "Üniversite", true),

            // Kamu & Kariyer (Yetişkin)
            new(150, C, "KPSS – Genel Yetenek / Genel Kültür",     "Yetişkin",   true),
            new(151, C, "KPSS – Eğitim Bilimleri",                  "Yetişkin",   true),
            new(152, C, "ÖABT – Öğretmenlik Alan Bilgisi",          "Yetişkin",   true),
            new(153, C, "EKYS – Eğitim Kurumları Yöneticisi",       "Yetişkin",   true),
            new(154, C, "Hakimlik ve Savcılık Sınavı",              "Yetişkin",   true),
            new(155, C, "Avukatlık (Baro) Sınavı",                  "Yetişkin",   true),
            new(156, C, "Noterlik Sınavı",                           "Yetişkin",   true),
            new(157, C, "Uzman Erbaş Sınavı",                       "Yetişkin",   true),
            new(158, C, "Subay / Astsubay Sınavları",               "Yetişkin",   true),
            new(159, C, "Vergi Müfettişi Sınavı",                   "Yetişkin",   true),
            new(160, C, "Gümrük Müfettişi Sınavı",                  "Yetişkin",   true),

            // Mesleki Sertifikalar (Yetişkin)
            new(165, C, "SMMM – Serbest Muhasebeci Mali Müşavirlik","Yetişkin",   true),
            new(166, C, "YMM – Yeminli Mali Müşavirlik",            "Yetişkin",   true),
            new(167, C, "İSG – İş Sağlığı ve Güvenliği Uzmanlığı","Yetişkin",   true),
            new(168, C, "SPK – Sermaye Piyasası Lisanslama",        "Yetişkin",   true),
            new(169, C, "Aktüerlik Sınavı",                         "Yetişkin",   true),
            new(170, C, "MYK – Mesleki Yeterlilik Sınavları",      "Yetişkin",   true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  ULUSLARARASI SINAVLAR  (Codes 200 – 249)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetUluslararasiSinavlar()
    {
        const string C = "Uluslararası Sınavlar";
        return
        [
            // İngilizce Dil Sınavları
            new(200, C, "IELTS Academic",                   "Her Seviye", true),
            new(201, C, "IELTS General Training",           "Her Seviye", true),
            new(202, C, "TOEFL iBT",                        "Her Seviye", true),
            new(203, C, "Cambridge B2 First (FCE)",         "Her Seviye", true),
            new(204, C, "Cambridge C1 Advanced (CAE)",      "Her Seviye", true),
            new(205, C, "Cambridge C2 Proficiency (CPE)",   "Her Seviye", true),
            new(206, C, "PTE Academic",                     "Her Seviye", true),
            new(207, C, "Duolingo English Test",            "Her Seviye", true),
            new(208, C, "TOEIC – İş İngilizcesi",          "Yetişkin",   true),
            new(209, C, "OET – Occupational English Test",  "Yetişkin",   true),
            new(210, C, "ITEP",                             "Her Seviye", true),

            // Üniversite Giriş
            new(215, C, "SAT – Matematik + Okuma-Yazma",   "Lise",       true),
            new(216, C, "ACT",                              "Lise",       true),
            new(217, C, "GRE – Graduate Record Exam",      "Üniversite", true),
            new(218, C, "GMAT – İşletme Yüksek Lisans",   "Üniversite", true),
            new(219, C, "LSAT – Hukuk Okulu Giriş",       "Üniversite", true),
            new(220, C, "MCAT – Tıp Okulu Giriş",         "Üniversite", true),

            // Uluslararası Müfredat
            new(225, C, "IB – International Baccalaureate","Lise",       true),
            new(226, C, "IGCSE",                           "Lise",       true),
            new(227, C, "A-Levels",                        "Lise",       true),
            new(228, C, "AP – Advanced Placement",         "Lise",       true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  ALMANCA SINAVLARI  (Codes 250 – 299)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetAlmancaSinavlari()
    {
        const string C = "Almanca Sınavları";
        return
        [
            // Goethe-Institut
            new(250, C, "Goethe A1 – Start Deutsch 1",                             "Her Seviye", true),
            new(251, C, "Goethe A2",                                                "Her Seviye", true),
            new(252, C, "Goethe B1",                                                "Her Seviye", true),
            new(253, C, "Goethe B2",                                                "Her Seviye", true),
            new(254, C, "Goethe C1",                                                "Her Seviye", true),
            new(255, C, "Goethe C2 – Großes Deutsches Sprachdiplom",               "Her Seviye", true),

            // Üniversite
            new(260, C, "TestDaF – Test Deutsch als Fremdsprache",                  "Üniversite", true),
            new(261, C, "DSH – Deutsche Sprachprüfung für den Hochschulzugang",    "Üniversite", true),

            // Telc
            new(265, C, "Telc Deutsch A1",                                          "Her Seviye", true),
            new(266, C, "Telc Deutsch A2",                                          "Her Seviye", true),
            new(267, C, "Telc Deutsch B1",                                          "Her Seviye", true),
            new(268, C, "Telc Deutsch B2",                                          "Her Seviye", true),
            new(269, C, "Telc Deutsch C1",                                          "Her Seviye", true),
            new(270, C, "Telc Deutsch B1+ Beruf – İş Almancası",                  "Yetişkin",   true),
            new(271, C, "Telc Deutsch B2+ Beruf – İş Almancası",                  "Yetişkin",   true),

            // Diğer
            new(275, C, "ÖSD – Österreichisches Sprachdiplom Deutsch",             "Her Seviye", true),
            new(276, C, "DSD – Deutsches Sprachdiplom (Okul)",                     "Lise",       true),
            new(277, C, "Abitur Hazırlık",                                          "Lise",       true),
            new(278, C, "Almanca Vizesi Dil Sınavı (A1)",                          "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  DİĞER DİL SERTİFİKA SINAVLARI  (Codes 300 – 359)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetDigerDilSertifikalari()
    {
        const string C = "Dil Sertifika Sınavları";
        return
        [
            // Fransızca
            new(300, C, "DELF A1 – A2 (Fransızca)",                "Her Seviye", true),
            new(301, C, "DELF B1 – B2 (Fransızca)",                "Her Seviye", true),
            new(302, C, "DALF C1 – C2 (Fransızca)",                "Her Seviye", true),
            new(303, C, "TCF – Test de Connaissance du Français",   "Her Seviye", true),
            new(304, C, "TEF – Test d'Évaluation de Français",     "Her Seviye", true),

            // İspanyolca
            new(310, C, "DELE A1 – A2 (İspanyolca)",               "Her Seviye", true),
            new(311, C, "DELE B1 – B2 (İspanyolca)",               "Her Seviye", true),
            new(312, C, "DELE C1 – C2 (İspanyolca)",               "Her Seviye", true),
            new(313, C, "SIELE – İspanyolca Çevrimiçi",            "Her Seviye", true),

            // İtalyanca
            new(318, C, "CILS – Certificazione İtalyanca",          "Her Seviye", true),
            new(319, C, "CELI – Certificato İtalyanca",             "Her Seviye", true),
            new(320, C, "PLIDA – İtalyanca Yeterlilik",             "Her Seviye", true),

            // Portekizce
            new(325, C, "CELPE-BRAS (Portekizce – Brezilya)",       "Her Seviye", true),

            // Japonca – JLPT
            new(330, C, "JLPT N5 – Japonca Başlangıç",             "Her Seviye", true),
            new(331, C, "JLPT N4 – Japonca",                       "Her Seviye", true),
            new(332, C, "JLPT N3 – Japonca",                       "Her Seviye", true),
            new(333, C, "JLPT N2 – Japonca",                       "Her Seviye", true),
            new(334, C, "JLPT N1 – Japonca İleri",                 "Her Seviye", true),

            // Çince – HSK
            new(338, C, "HSK 1 – 2 (Çince Başlangıç)",            "Her Seviye", true),
            new(339, C, "HSK 3 – 4 (Çince Orta)",                 "Her Seviye", true),
            new(340, C, "HSK 5 – 6 (Çince İleri)",               "Her Seviye", true),
            new(341, C, "HSKK – Çince Konuşma Sınavı",            "Her Seviye", true),

            // Korece – TOPIK
            new(345, C, "TOPIK I (Korece 1–2. Seviye)",            "Her Seviye", true),
            new(346, C, "TOPIK II (Korece 3–6. Seviye)",           "Her Seviye", true),

            // Rusça
            new(350, C, "TRKI / TORFL – Rusça Yeterlilik",         "Her Seviye", true),

            // Arapça
            new(354, C, "ALPT – Arapça Dil Yeterlilik Sınavı",     "Her Seviye", true),

            // Çocuk İngilizcesi
            new(358, C, "Cambridge YLE – Çocuklar İçin İngilizce", "İlkokul",    true),
            new(359, C, "Trinity College London – İngilizce",       "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  YABANCI DİLLER  (Codes 400 – 449)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetYabanciDiller()
    {
        const string C = "Yabancı Diller";
        return
        [
            new(400, C, "İngilizce",                    "İlkokul",    true),
            new(401, C, "İngilizce",                    "Ortaokul",   true),
            new(402, C, "İngilizce",                    "Lise",       true),
            new(403, C, "İngilizce",                    "Üniversite", true),
            new(404, C, "İngilizce",                    "Yetişkin",   true),
            new(405, C, "Almanca",                      "Her Seviye", true),
            new(406, C, "Fransızca",                    "Her Seviye", true),
            new(407, C, "Rusça",                        "Her Seviye", true),
            new(408, C, "Arapça",                       "Her Seviye", true),
            new(409, C, "İspanyolca",                   "Her Seviye", true),
            new(410, C, "İtalyanca",                    "Her Seviye", true),
            new(411, C, "Japonca",                      "Her Seviye", true),
            new(412, C, "Çince (Mandarin)",             "Her Seviye", true),
            new(413, C, "Portekizce",                   "Her Seviye", true),
            new(414, C, "Korece",                       "Her Seviye", true),
            new(415, C, "Farsça",                       "Her Seviye", true),
            new(416, C, "Yunanca",                      "Her Seviye", true),
            new(417, C, "Hollandaca",                   "Her Seviye", true),
            new(418, C, "Norveçce",                     "Her Seviye", true),
            new(419, C, "Lehçe",                        "Her Seviye", true),
            new(420, C, "Ukraynaca",                    "Her Seviye", true),
            new(421, C, "Latince",                      "Her Seviye", true),
            new(422, C, "Hintçe (Hindice)",             "Her Seviye", true),
            new(423, C, "İbranice",                     "Her Seviye", true),
            new(424, C, "Türkçe (Yabancılara)",        "Her Seviye", true),
            new(425, C, "İngiliz Dili ve Edebiyatı",   "Üniversite", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  FEN BİLİMLERİ  (Codes 450 – 469)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetFenBilimleri()
    {
        const string C = "Fen Bilimleri";
        return
        [
            new(450, C, "Genel Fen Bilgisi",        "İlkokul",  true),
            new(451, C, "Genel Fen Bilimleri",      "Ortaokul", true),
            new(452, C, "Canlılar Dünyası",         "Ortaokul", true),
            new(453, C, "Madde ve Özellikleri",     "Ortaokul", true),
            new(454, C, "Kuvvet ve Hareket",        "Ortaokul", true),
            new(455, C, "Işık ve Ses",             "Ortaokul", true),
            new(456, C, "Elektrik (Fen)",           "Ortaokul", true),
            new(457, C, "İnsan Vücudu",             "Ortaokul", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  FİZİK  (Codes 470 – 489)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetFizik()
    {
        const string C = "Fizik";
        return
        [
            new(470, C, "Genel Fizik",              "Lise",       true),
            new(471, C, "Mekanik",                  "Lise",       true),
            new(472, C, "Elektromanyetizma",        "Lise",       true),
            new(473, C, "Optik",                    "Lise",       true),
            new(474, C, "Genel Fizik",              "Üniversite", true),
            new(475, C, "Mekanik",                  "Üniversite", true),
            new(476, C, "Termodinamik",             "Üniversite", true),
            new(477, C, "Elektromanyetizma",        "Üniversite", true),
            new(478, C, "Kuantum Fiziği",           "Üniversite", true),
            new(479, C, "Akışkanlar Mekaniği",     "Üniversite", true),
            new(480, C, "Mukavemet",                "Üniversite", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  KİMYA  (Codes 490 – 509)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetKimya()
    {
        const string C = "Kimya";
        return
        [
            new(490, C, "Genel Kimya",          "Lise",       true),
            new(491, C, "Genel Kimya",          "Üniversite", true),
            new(492, C, "Organik Kimya",        "Lise",       true),
            new(493, C, "Organik Kimya",        "Üniversite", true),
            new(494, C, "Analitik Kimya",       "Üniversite", true),
            new(495, C, "Biyokimya",            "Üniversite", true),
            new(496, C, "Fizikokimya",          "Üniversite", true),
            new(497, C, "Çevre Kimyası",       "Üniversite", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  BİYOLOJİ  (Codes 510 – 529)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetBiyoloji()
    {
        const string C = "Biyoloji";
        return
        [
            new(510, C, "Genel Biyoloji",       "Lise",       true),
            new(511, C, "Genel Biyoloji",       "Üniversite", true),
            new(512, C, "Genetik",              "Lise",       true),
            new(513, C, "Genetik",              "Üniversite", true),
            new(514, C, "Hücre Biyolojisi",    "Üniversite", true),
            new(515, C, "Mikrobiyoloji",        "Üniversite", true),
            new(516, C, "Fizyoloji",            "Üniversite", true),
            new(517, C, "Anatomi",              "Üniversite", true),
            new(518, C, "Ekoloji",              "Üniversite", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  TÜRKÇE VE EDEBİYAT  (Codes 530 – 549)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetTurkceEdebiyat()
    {
        const string C = "Türkçe ve Edebiyat";
        return
        [
            new(530, C, "Türkçe",                       "İlkokul",    true),
            new(531, C, "Türkçe",                       "Ortaokul",   true),
            new(532, C, "Dil ve Anlatım",               "Lise",       true),
            new(533, C, "Türk Dili ve Edebiyatı",       "Lise",       true),
            new(534, C, "Türk Dili ve Edebiyatı",       "Üniversite", true),
            new(535, C, "Osmanlı Türkçesi",             "Üniversite", true),
            new(536, C, "Diksiyon",                     "Her Seviye", true),
            new(537, C, "Yaratıcı Yazarlık",            "Her Seviye", true),
            new(538, C, "Hızlı Okuma Teknikleri",      "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SOSYAL BİLGİLER / TARİH / COĞRAFYA  (Codes 550 – 579)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetSosyalTarihCografya()
    {
        return
        [
            new(550, "Sosyal Bilgiler", "Sosyal Bilgiler",                  "Ortaokul",   true),
            new(551, "Sosyal Bilgiler", "Vatandaşlık Bilgisi",              "Ortaokul",   true),
            new(552, "Sosyal Bilgiler", "Hayat Bilgisi",                    "İlkokul",    true),

            new(560, "Tarih",           "Türk Tarihi",                      "Lise",       true),
            new(561, "Tarih",           "Dünya Tarihi",                     "Lise",       true),
            new(562, "Tarih",           "Osmanlı Tarihi",                   "Lise",       true),
            new(563, "Tarih",           "İnkılap Tarihi ve Atatürkçülük",  "Lise",       true),
            new(564, "Tarih",           "Türk Tarihi",                      "Üniversite", true),

            new(570, "Coğrafya",        "Genel Coğrafya",                   "Lise",       true),
            new(571, "Coğrafya",        "Fiziki Coğrafya",                  "Lise",       true),
            new(572, "Coğrafya",        "Beşeri ve İktisadi Coğrafya",     "Lise",       true),
            new(573, "Coğrafya",        "Türkiye Coğrafyası",               "Lise",       true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  FELSEFE / PSİKOLOJİ  (Codes 580 – 599)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetFelsefePsikoloji()
    {
        return
        [
            new(580, "Felsefe",   "Genel Felsefe",          "Lise",       true),
            new(581, "Felsefe",   "Mantık",                 "Lise",       true),
            new(582, "Felsefe",   "Etik (Ahlak Felsefesi)", "Lise",       true),
            new(583, "Felsefe",   "Genel Felsefe",          "Üniversite", true),
            new(584, "Felsefe",   "Epistemoloji",           "Üniversite", true),

            new(590, "Psikoloji", "Genel Psikoloji",        "Lise",       true),
            new(591, "Psikoloji", "Genel Psikoloji",        "Üniversite", true),
            new(592, "Psikoloji", "Sosyal Psikoloji",       "Üniversite", true),
            new(593, "Psikoloji", "Gelişim Psikolojisi",   "Üniversite", true),
            new(594, "Psikoloji", "Klinik Psikoloji",       "Üniversite", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  İSTATİSTİK / EKONOMİ / HUKUK  (Codes 600 – 639)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetIstatistikEkonomiHukuk()
    {
        return
        [
            new(600, "İstatistik",       "Genel İstatistik",             "Lise",       true),
            new(601, "İstatistik",       "Genel İstatistik",             "Üniversite", true),
            new(602, "İstatistik",       "Olasılık Teorisi",             "Üniversite", true),
            new(603, "İstatistik",       "SPSS",                         "Üniversite", true),
            new(604, "İstatistik",       "Veri Analizi",                 "Üniversite", true),

            new(610, "Ekonomi ve Hukuk", "Mikroekonomi",                 "Üniversite", true),
            new(611, "Ekonomi ve Hukuk", "Makroekonomi",                 "Üniversite", true),
            new(612, "Ekonomi ve Hukuk", "Ekonometri",                   "Üniversite", true),
            new(613, "Ekonomi ve Hukuk", "Maliye",                       "Üniversite", true),
            new(614, "Ekonomi ve Hukuk", "İşletme",                      "Lise",       true),
            new(615, "Ekonomi ve Hukuk", "İşletme",                      "Üniversite", true),
            new(620, "Ekonomi ve Hukuk", "Özel Hukuk",                   "Üniversite", true),
            new(621, "Ekonomi ve Hukuk", "Kamu Hukuku",                  "Üniversite", true),
            new(622, "Ekonomi ve Hukuk", "Ticaret Hukuku",               "Üniversite", true),
            new(623, "Ekonomi ve Hukuk", "İş Hukuku",                    "Üniversite", true),
            new(624, "Ekonomi ve Hukuk", "Ceza Hukuku",                  "Üniversite", true),
            new(625, "Ekonomi ve Hukuk", "Anayasa Hukuku",               "Üniversite", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  TEMEL EĞİTİM  (Codes 700 – 719)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetTemelEgitim()
    {
        const string C = "Temel Eğitim";
        return
        [
            new(700, C, "Genel Takviye",                    "İlkokul",  true),
            new(701, C, "Okuma Yazma",                      "İlkokul",  true),
            new(702, C, "İlkokul Matematik",               "İlkokul",  true),
            new(703, C, "Okuma Yazmaya Hazırlık",          "İlkokul",  true),
            new(704, C, "Montessori Eğitimi",              "İlkokul",  true),
            new(705, C, "Genel Takviye",                    "Ortaokul", true),
            new(706, C, "Ödev Destek",                      "İlkokul",  true),
            new(707, C, "Ödev Destek",                      "Ortaokul", true),
            new(708, C, "Okul Öncesi Eğitim",             "İlkokul",  true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  ÖZEL EĞİTİM  (Codes 720 – 739)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetOzelEgitim()
    {
        const string C = "Özel Eğitim";
        return
        [
            new(720, C, "Otizm Spektrum Bozukluğu",             "Her Seviye", true),
            new(721, C, "Öğrenme Güçlüğü (Disleksi vb.)",     "Her Seviye", true),
            new(722, C, "DEHB – Dikkat Eksikliği",             "Her Seviye", true),
            new(723, C, "Zihinsel Yetersizlik",                 "Her Seviye", true),
            new(724, C, "Görme / İşitme Yetersizliği",        "Her Seviye", true),
            new(725, C, "Dil ve Konuşma Bozuklukları",        "Her Seviye", true),
            new(726, C, "Üstün Zekalı Eğitim",                "Her Seviye", true),
            new(727, C, "Gölge Öğretmenlik",                   "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  ÜNİVERSİTE TAKVİYE  (Codes 740 – 749)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetUniversiteTakviye()
    {
        const string C = "Üniversite Dersleri Takviye";
        return
        [
            new(740, C, "Mühendislik Matematiği",               "Üniversite", true),
            new(741, C, "İnşaat Mühendisliği Dersleri",        "Üniversite", true),
            new(742, C, "Elektrik-Elektronik Müh. Dersleri",    "Üniversite", true),
            new(743, C, "Makine Mühendisliği Dersleri",         "Üniversite", true),
            new(744, C, "Tıp Fakültesi Dersleri",               "Üniversite", true),
            new(745, C, "Tez Yazımı Danışmanlığı",             "Üniversite", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  MÜZİK  (Codes 800 – 839)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetMuzik()
    {
        const string C = "Müzik";
        return
        [
            new(800, C, "Piyano",                   "Her Seviye", true),
            new(801, C, "Gitar",                    "Her Seviye", true),
            new(802, C, "Klasik Gitar",             "Her Seviye", true),
            new(803, C, "Keman",                    "Her Seviye", true),
            new(804, C, "Bağlama",                  "Her Seviye", true),
            new(805, C, "Flüt",                    "Her Seviye", true),
            new(806, C, "Saksafon",                 "Her Seviye", true),
            new(807, C, "Klarnet",                  "Her Seviye", true),
            new(808, C, "Bateri / Davul",           "Her Seviye", true),
            new(809, C, "Viyolonsel (Cello)",       "Her Seviye", true),
            new(810, C, "Ney",                      "Her Seviye", true),
            new(811, C, "Ud",                       "Her Seviye", true),
            new(812, C, "Kanun",                    "Her Seviye", true),
            new(813, C, "Ukulele",                  "Her Seviye", true),
            new(814, C, "Solfej",                   "Her Seviye", true),
            new(815, C, "Müzik Teorisi",           "Her Seviye", true),
            new(816, C, "Ses Eğitimi (Vokal)",     "Her Seviye", true),
            new(817, C, "Şan (Opera / Vokal)",    "Her Seviye", true),
            new(818, C, "Jazz",                     "Her Seviye", true),
            new(819, C, "Makam Müziği",           "Her Seviye", true),
            new(820, C, "DJ Eğitimi",             "Her Seviye", true),
            new(821, C, "Armoni ve Kontrpuan",     "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SPOR  (Codes 850 – 889)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetSpor()
    {
        const string C = "Spor";
        return
        [
            new(850, C, "Yüzme",            "Her Seviye", true),
            new(851, C, "Futbol",           "Her Seviye", true),
            new(852, C, "Basketbol",        "Her Seviye", true),
            new(853, C, "Voleybol",         "Her Seviye", true),
            new(854, C, "Tenis",            "Her Seviye", true),
            new(855, C, "Masa Tenisi",      "Her Seviye", true),
            new(856, C, "Badminton",        "Her Seviye", true),
            new(857, C, "Fitness",          "Her Seviye", true),
            new(858, C, "Yoga",             "Her Seviye", true),
            new(859, C, "Pilates",          "Her Seviye", true),
            new(860, C, "Zumba",            "Her Seviye", true),
            new(861, C, "Karate",           "Her Seviye", true),
            new(862, C, "Taekwondo",        "Her Seviye", true),
            new(863, C, "Judo",             "Her Seviye", true),
            new(864, C, "Boks",             "Her Seviye", true),
            new(865, C, "Kickboks",         "Her Seviye", true),
            new(866, C, "Muay Thai",        "Her Seviye", true),
            new(867, C, "Jiu-Jitsu",        "Her Seviye", true),
            new(868, C, "Jimnastik",        "Her Seviye", true),
            new(869, C, "Binicilik",        "Her Seviye", true),
            new(870, C, "Buz Pateni",       "Her Seviye", true),
            new(871, C, "Bisiklet",         "Her Seviye", true),
            new(872, C, "Okçuluk",         "Her Seviye", true),
            new(873, C, "Krav Maga",        "Her Seviye", true),
            new(874, C, "Personal Trainer", "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  DANS  (Codes 900 – 919)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetDans()
    {
        const string C = "Dans";
        return
        [
            new(900, C, "Bale",             "Her Seviye", true),
            new(901, C, "Halk Oyunları",   "Her Seviye", true),
            new(902, C, "Modern Dans",      "Her Seviye", true),
            new(903, C, "Oryantal Dans",    "Her Seviye", true),
            new(904, C, "Salsa",            "Her Seviye", true),
            new(905, C, "Bachata",          "Her Seviye", true),
            new(906, C, "Tango",            "Her Seviye", true),
            new(907, C, "Break Dans",       "Her Seviye", true),
            new(908, C, "Jazz Dans",        "Her Seviye", true),
            new(909, C, "Hip-Hop Dans",     "Her Seviye", true),
            new(910, C, "Vals",             "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SANAT VE EL SANATLARI  (Codes 930 – 949)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetSanat()
    {
        const string C = "Sanat ve El Sanatları";
        return
        [
            new(930, C, "Resim",                        "Her Seviye", true),
            new(931, C, "Yağlı Boya",                 "Her Seviye", true),
            new(932, C, "Kara Kalem",                  "Her Seviye", true),
            new(933, C, "Grafik Tasarım",               "Her Seviye", true),
            new(934, C, "Fotoğrafçılık",               "Her Seviye", true),
            new(935, C, "Seramik ve Çini",             "Her Seviye", true),
            new(936, C, "Ebru Sanatı",                 "Her Seviye", true),
            new(937, C, "Kaligrafi",                    "Her Seviye", true),
            new(938, C, "Takı Tasarımı",               "Her Seviye", true),
            new(939, C, "Dikiş ve Nakış",              "Her Seviye", true),
            new(940, C, "Moda Tasarımı",               "Her Seviye", true),
            new(941, C, "Oyunculuk ve Tiyatro",         "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  BİLİŞİM TEKNOLOJİLERİ  (Codes 1000 – 1049)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetBilisimTeknolojileri()
    {
        const string C = "Bilişim Teknolojileri";
        return
        [
            new(1000, C, "Python",                          "Lise",       true),
            new(1001, C, "Python",                          "Üniversite", true),
            new(1002, C, "Python",                          "Yetişkin",   true),
            new(1003, C, "Java",                            "Üniversite", true),
            new(1004, C, "C#",                              "Üniversite", true),
            new(1005, C, "C++",                             "Üniversite", true),
            new(1006, C, "PHP",                             "Her Seviye", true),
            new(1007, C, "SQL / Veritabanı",                "Her Seviye", true),
            new(1008, C, "HTML & CSS Web Geliştirme",      "Her Seviye", true),
            new(1009, C, "Algoritma ve Veri Yapıları",      "Üniversite", true),
            new(1010, C, "Yapay Zeka ve Makine Öğrenmesi", "Üniversite", true),
            new(1011, C, "Mobil Uygulama Geliştirme",       "Her Seviye", true),
            new(1012, C, "Siber Güvenlik",                  "Her Seviye", true),
            new(1013, C, "MATLAB",                          "Üniversite", true),
            new(1014, C, "AutoCAD",                         "Her Seviye", true),
            new(1015, C, "SolidWorks",                      "Üniversite", true),
            new(1016, C, "Revit",                           "Her Seviye", true),
            new(1017, C, "Photoshop",                       "Her Seviye", true),
            new(1018, C, "Illustrator",                     "Her Seviye", true),
            new(1019, C, "Adobe Premiere Pro",              "Her Seviye", true),
            new(1020, C, "After Effects",                   "Her Seviye", true),
            new(1021, C, "Blender 3D Modelleme",            "Her Seviye", true),
            new(1022, C, "Microsoft Excel",                 "Her Seviye", true),
            new(1023, C, "Microsoft Office Programları",    "Her Seviye", true),
            new(1024, C, "SEO",                             "Her Seviye", true),
            new(1025, C, "Dijital Pazarlama",               "Her Seviye", true),
            new(1026, C, "Unity – Oyun Geliştirme",        "Her Seviye", true),
            new(1027, C, "WordPress",                       "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  ROBOTİK VE KODLAMA  (Codes 1060 – 1079)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetRobotikKodlama()
    {
        const string C = "Robotik ve Kodlama";
        return
        [
            new(1060, C, "Robotik Kodlama (Genel)",         "Her Seviye", true),
            new(1061, C, "Arduino Programlama",             "Her Seviye", true),
            new(1062, C, "Scratch – Görsel Kodlama",        "İlkokul",    true),
            new(1063, C, "Scratch – Görsel Kodlama",        "Ortaokul",   true),
            new(1064, C, "Lego Robotik",                    "İlkokul",    true),
            new(1065, C, "Lego Robotik",                    "Ortaokul",   true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  DANIŞMANLIK VE KOÇLUK  (Codes 1100 – 1119)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetDanismanlikKocluk()
    {
        const string C = "Danışmanlık ve Koçluk";
        return
        [
            new(1100, C, "Eğitim Koçluğu",                 "Her Seviye", true),
            new(1101, C, "Kariyer Koçluğu",                "Yetişkin",   true),
            new(1102, C, "Yaşam Koçluğu",                  "Yetişkin",   true),
            new(1103, C, "Aile Danışmanlığı",              "Yetişkin",   true),
            new(1104, C, "NLP",                             "Yetişkin",   true),
            new(1105, C, "Etkili İletişim",               "Her Seviye", true),
            new(1106, C, "Sunum Teknikleri",               "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  MUHASEBE VE FİNANS  (Codes 1130 – 1149)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetMuhasebeFinans()
    {
        const string C = "Muhasebe ve Finans";
        return
        [
            new(1130, C, "Genel Muhasebe",          "Üniversite", true),
            new(1131, C, "Genel Muhasebe",          "Yetişkin",   true),
            new(1132, C, "Maliyet Muhasebesi",      "Üniversite", true),
            new(1133, C, "Finans",                  "Üniversite", true),
            new(1134, C, "Denetim",                 "Üniversite", true),
            new(1135, C, "Vergi Hukuku",            "Yetişkin",   true),
            new(1136, C, "Bütçe Yönetimi",         "Yetişkin",   true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  KİŞİSEL GELİŞİM  (Codes 1150 – 1169)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetKisiselGelisim()
    {
        const string C = "Kişisel Gelişim";
        return
        [
            new(1150, C, "Zaman Yönetimi",              "Her Seviye", true),
            new(1151, C, "Stres Yönetimi",              "Her Seviye", true),
            new(1152, C, "Liderlik Becerileri",         "Her Seviye", true),
            new(1153, C, "Hızlı Okuma",                "Her Seviye", true),
            new(1154, C, "Bellek ve Hafıza Teknikleri", "Her Seviye", true),
            new(1155, C, "Meditasyon",                  "Her Seviye", true),
            new(1156, C, "İşaret Dili",                "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  SAĞLIK VE YAŞAM  (Codes 1200 – 1219)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetSaglikYasam()
    {
        const string C = "Sağlık ve Yaşam";
        return
        [
            new(1200, C, "Beslenme ve Diyetisyenlik",   "Her Seviye", true),
            new(1201, C, "İlk Yardım",                  "Her Seviye", true),
            new(1202, C, "Makyaj",                      "Her Seviye", true),
            new(1203, C, "Cilt Bakımı",                 "Her Seviye", true),
            new(1204, C, "Kuaförlük ve Saç Bakımı",    "Her Seviye", true),
            new(1205, C, "Türk Mutfağı",                "Her Seviye", true),
            new(1206, C, "Pastacılık ve Fırıncılık",   "Her Seviye", true),
            new(1207, C, "Barista / Kahve Sanatı",      "Her Seviye", true),
            new(1208, C, "Tur Rehberliği",              "Her Seviye", true),
            new(1209, C, "Direksiyon Eğitimi",          "Her Seviye", true),
        ];
    }

    // ═══════════════════════════════════════════════════════════════════
    //  HOBİ VE DİĞER  (Codes 1250 – 1269)
    // ═══════════════════════════════════════════════════════════════════
    private static IEnumerable<SubjectSeedItem> GetHobiDiger()
    {
        const string C = "Hobi ve Diğer";
        return
        [
            new(1250, C, "Satranç",                             "Her Seviye", true),
            new(1251, C, "Rubik Küp",                           "Her Seviye", true),
            new(1252, C, "Kuran-ı Kerim Okuma",                "Her Seviye", true),
            new(1253, C, "Tecvid",                              "Her Seviye", true),
            new(1254, C, "Arapça (Dini Eğitim)",              "Her Seviye", true),
            new(1255, C, "Müzik Yetenek Sınavı Hazırlığı",    "Lise",       true),
            new(1256, C, "Güzel Sanatlar Sınavına Hazırlık",  "Lise",       true),
        ];
    }

    // ───────────────────────────────────────────────────────────────────
    //  PRIVATE RECORD — DepartmentSeedItem ile aynı pattern
    // ───────────────────────────────────────────────────────────────────
    private sealed record SubjectSeedItem(int Code, string Category, string Name, string Level, bool IsActive);
}