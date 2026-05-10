using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Infrastructure.Services.Email;

public class EmailOptions
{
    public string Provider { get; set; } = "Brevo";
    public string ApiKey { get; set; } = default!;
    public string FromEmail { get; set; } = default!;
    public string FromName { get; set; } = "TeacherFind";
}