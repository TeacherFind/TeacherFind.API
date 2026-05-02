using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Domain.Entities;

public class Subject
{
    public int Id { get; set; }

    /// <summary>
    /// Seed eşleştirmesi için sabit int kodu.
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// Üst kategori adı. Örn: "Matematik", "Türkiye Sınavları", "Müzik"
    /// </summary>
    public string Category { get; set; } = default!;

    /// <summary>
    /// Ders / sınav adı. Örn: "Geometri", "YDS – İngilizce", "Piyano"
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Hedef öğrenci seviyesi. Örn: "İlkokul", "Ortaokul", "Lise", "Üniversite", "Her Seviye"
    /// </summary>
    public string Level { get; set; } = default!;

    public bool IsActive { get; set; } = true;
}