using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Listings
{
    public class CertificateDto
    {
        public string SertifikaAdi { get; set; } = default!;
        public string VerenKurum { get; set; } = default!;
        public int Yil { get; set; }
    }
}
