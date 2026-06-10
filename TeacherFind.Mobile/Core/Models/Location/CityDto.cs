using System;

namespace TeacherFind.Mobile.Core.Models.Location
{
    public class CityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int PlateCode { get; set; }
    }
}
