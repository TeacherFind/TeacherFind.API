using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Admin;

public class AdminActionLogDto
{
    public Guid Id { get; set; }

    public Guid AdminUserId { get; set; }

    public string AdminFullName { get; set; } = default!;

    public string AdminEmail { get; set; } = default!;

    public string Action { get; set; } = default!;

    public string EntityName { get; set; } = default!;

    public Guid? EntityId { get; set; }

    public string? Description { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }
}