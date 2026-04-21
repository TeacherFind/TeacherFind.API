using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;


namespace TeacherFind.Infrastructure.Persistence.Repositories
{
    public class VerificationRepository : IVerificationRepository
    {
        private readonly AppDbContext _context;

        public VerificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<VerificationCode?> GetValidCode(Guid userId, string code)
        {
            return await _context.VerificationCodes
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.Code == code &&
                    !x.IsUsed &&
                    x.ExpireAt > DateTime.UtcNow);
        }

        public async Task AddAsync(VerificationCode verificationCode)
        {
            await _context.VerificationCodes.AddAsync(verificationCode);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
