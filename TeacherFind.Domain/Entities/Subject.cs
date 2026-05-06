using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Domain.Entities;

public class Subject
{
    public int Id { get; set; }

    /// <summary>Seed eşleştirmesi için sabit int kodu.</summary>
    public int Code { get; set; }

    /// <summary>Hedef öğrenci grubu. Örn: "İlk-Ortaöğretim", "Lise", "Üniversite", "Yetişkin", "Her Seviye"</summary>
    public string Stage { get; set; } = default!;

    /// <summary>Üst kategori. Örn: "Matematik", "Fen Bilimleri", "Yabancı Diller"</summary>
    public string Category { get; set; } = default!;

    /// <summary>Ders adı. Örn: "TYT Matematik", "Geometri", "İngilizce"</summary>
    public string Name { get; set; } = default!;

    /// <summary>Seviye. Örn: "Temel", "Orta", "İleri", "Her Seviye"</summary>
    public string Level { get; set; } = default!;

    public bool IsActive { get; set; } = true;
}