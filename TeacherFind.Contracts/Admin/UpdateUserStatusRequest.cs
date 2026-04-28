using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Admin;

public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}