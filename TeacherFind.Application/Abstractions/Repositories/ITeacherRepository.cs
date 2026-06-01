using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface ITeacherRepository
{
    Task<TeacherProfile?> GetByUserIdAsync(Guid userId);
    Task<TeacherProfile?> GetByUserIdWithCertificatesAsync(Guid userId);
    Task<TeacherProfile?> GetByUserIdWithAvailabilitiesAsync(Guid userId);
    Task<TeacherProfile?> GetByUserIdWithFullDetailsAsync(Guid userId);   // NEW
    Task<TeacherProfile?> GetByIdAsync(Guid id);

    Task<TeacherCertificate?> GetCertificateForUserAsync(Guid userId, Guid certificateId);
    Task<TeacherAvailability?> GetAvailabilityForUserAsync(Guid userId, Guid availabilityId);

    Task AddAsync(TeacherProfile teacher);
    Task AddCertificateAsync(TeacherCertificate certificate);
    void RemoveCertificate(TeacherCertificate certificate);

    Task ReplaceAvailabilitiesAsync(Guid teacherProfileId, List<TeacherAvailability> availabilities);
    void RemoveAvailability(TeacherAvailability availability);

    void Update(TeacherProfile profile);
    Task UpdateAsync(TeacherProfile profile);
    Task SaveChangesAsync();
}