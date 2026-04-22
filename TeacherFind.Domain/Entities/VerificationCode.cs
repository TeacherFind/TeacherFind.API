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

        public string Code { get; set; }

        public string Type { get; set; } // "Phone" | "Email"

        public DateTime ExpireAt { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}
