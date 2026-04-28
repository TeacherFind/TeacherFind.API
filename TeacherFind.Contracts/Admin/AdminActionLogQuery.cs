using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Admin;

public class AdminActionLogQuery
{
    public string? Action { get; set; }

    public string? EntityName { get; set; }

    public Guid? AdminUserId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
