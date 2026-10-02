using System.ComponentModel.DataAnnotations;
namespace UniversityApi;

public class EnrollmentRequest
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }
    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }
    [Required]
    public DateOnly? EnrolledOn { get; set; }
}
