using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Domain.Entities
{
    public class VerificationCode
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = default!;

        public string Code { get; set; } = default!;

        public string Type { get; set; } = default!; // "Phone" | "Email"

        // Kodun hangi hedef değer için üretildiğini saklar (örn. e-posta değişikliğinde yeni e-posta).
        // Doğrulamada gelen değerle eşleştirilir; null ise hedef bağlama gerektirmeyen kodlar içindir.
        public string? TargetValue { get; set; }

        public DateTime ExpireAt { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}