using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

public interface IVerificationRepository
{
    Task<VerificationCode?> GetValidCode(Guid userId, string code);
    Task AddAsync(VerificationCode verificationCode);
    Task SaveChangesAsync();
}
