using System;

namespace TeacherFind.Mobile.Core.Models.Location
{
    public class DistrictDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
