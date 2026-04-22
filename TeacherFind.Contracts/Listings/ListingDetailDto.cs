using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Listings;

public class ListingDetailDto
{
    public Guid IlanId { get; set; }
    public string IlanBasligi { get; set; } = "";
    public string IlanAciklamasi { get; set; } = "";
    public string Kategori { get; set; } = "";
    public string AltKategori { get; set; } = "";
    public string DersTipi { get; set; } = "";
    public int DersSuresi { get; set; }
    public decimal SaatlikFiyat { get; set; }
    public int ViewCount { get; set; }

    public Guid OgretmenId { get; set; }
    public string OgretmenAdi { get; set; } = "";
    public string? OgretmenFotografi { get; set; }
    public string? OgretmenBasligi { get; set; }
    public string? OgretmenBio { get; set; }
    public string? OgretmenSehir { get; set; }
    public double OgretmenPuani { get; set; }
    public int YorumSayisi { get; set; }

    public List<CertificateDto> Sertifikalar { get; set; } = new();
    public List<AvailabilityDto> MusaitGunler { get; set; } = new();

    public string IlanDurumu { get; set; } = "";
    public DateTime OlusturulmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
}