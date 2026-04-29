using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Reports;

public class CreateReportDto
{
    /// <summary>Şikayet edilen ilan ID (opsiyonel)</summary>
    public Guid? TargetListingId { get; set; }

    /// <summary>Şikayet edilen kullanıcı ID (opsiyonel)</summary>
    public Guid? TargetUserId { get; set; }

    /// <summary>Örnek: "Uygunsuz içerik", "Sahte ilan", "Hakaret"</summary>
    public string Reason { get; set; } = default!;

    public string? Description { get; set; }
}