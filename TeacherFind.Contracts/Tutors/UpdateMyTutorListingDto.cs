using System.ComponentModel.DataAnnotations;

namespace TeacherFind.Contracts.Tutors;

public class UpdateMyTutorListingDto : IValidatableObject
{
    public int? SubjectId { get; set; }

    public Guid? CityId { get; set; }

    public Guid? DistrictId { get; set; }

    public Guid? NeighborhoodId { get; set; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string SubCategory { get; set; } = default!;

    public int LessonDuration { get; set; }

    [Range(300, 5000, ErrorMessage = "Fiyat 300 TL ile 5000 TL arasında olmalıdır.")]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(1, 3, ErrorMessage = "Geçerli bir ders türü seçiniz. 1: Online, 2: FaceToFace, 3: Both")]
    public int ServiceType { get; set; } = 3;


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Price % 50 != 0)
        {
            yield return new ValidationResult(
                "Fiyat 50 TL'nin katı olmalıdır.",
                new[] { nameof(Price) });
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult(
                "İlan başlığı zorunludur.",
                new[] { nameof(Title) });
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            yield return new ValidationResult(
                "İlan açıklaması zorunludur.",
                new[] { nameof(Description) });
        }

        if (string.IsNullOrWhiteSpace(Category))
        {
            yield return new ValidationResult(
                "Kategori zorunludur.",
                new[] { nameof(Category) });
        }

        if (string.IsNullOrWhiteSpace(SubCategory))
        {
            yield return new ValidationResult(
                "Alt kategori zorunludur.",
                new[] { nameof(SubCategory) });
        }

        if (LessonDuration <= 0)
        {
            yield return new ValidationResult(
                "Ders süresi sıfırdan büyük olmalıdır.",
                new[] { nameof(LessonDuration) });
        }
    }
}