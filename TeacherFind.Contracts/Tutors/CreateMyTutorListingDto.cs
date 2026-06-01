using System.ComponentModel.DataAnnotations;

namespace TeacherFind.Contracts.Tutors;

public class CreateMyTutorListingDto : IValidatableObject
{
    public int? SubjectId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? DistrictId { get; set; }
    public Guid? NeighborhoodId { get; set; }

    [Required(ErrorMessage = "Başlık zorunludur.")]
    [MaxLength(150)]
    public string Title { get; set; } = default!;

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    [MaxLength(2000)]
    public string Description { get; set; } = default!;

    [Required(ErrorMessage = "Kategori zorunludur.")]
    [MaxLength(100)]
    public string Category { get; set; } = default!;

    [Required(ErrorMessage = "Alt kategori zorunludur.")]
    [MaxLength(100)]
    public string SubCategory { get; set; } = default!;

    [Range(1, 3, ErrorMessage = "Geçerli bir ders türü seçiniz. 1: Online, 2: FaceToFace, 3: Both")]
    public int ServiceType { get; set; } = 3;

    [Range(1, int.MaxValue, ErrorMessage = "Ders süresi 0'dan büyük olmalıdır.")]
    public int LessonDuration { get; set; }

    public decimal Price { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (Price < 300 || Price > 5000)
            yield return new ValidationResult(
                "Fiyat 300 ile 5000 TL arasında olmalıdır.",
                new[] { nameof(Price) });

        if (Price % 50 != 0)
            yield return new ValidationResult(
                "Fiyat 50 TL'nin katı olmalıdır.",
                new[] { nameof(Price) });
    }
}