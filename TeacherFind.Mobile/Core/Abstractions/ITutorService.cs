using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Tutors; // Contracts referansını kullanıyoruz
using TeacherFind.Mobile.Core.Models;

namespace TeacherFind.Mobile.Core.Abstractions;

public interface ITutorService
{
    Task<IEnumerable<TutorDto>> GetAllTutorsAsync();
    Task<TutorDto> GetTutorByIdAsync(Guid id);
    // Diğer metodlar zamanla eklenecek...
}
