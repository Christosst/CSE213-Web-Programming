using System.ComponentModel.DataAnnotations;

namespace UniversityWeb.Models;

// Form input excludes database IDs. The server loads the record addressed by the route.
public class StudentInput
{
    [Required, StringLength(200), Display(Name = "Full name")]
    public string FullName { get; set; } = "";
    [Required, StringLength(600), Display(Name = "Registration number")]
    public string RegistrationNumber { get; set; } = "";
    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }
}

public class TeacherInput
{
    [Required, StringLength(200), Display(Name = "Full name")]
    public string FullName { get; set; } = "";
    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }
}

public class CourseInput
{
    [Required, StringLength(50), Display(Name = "Course code")]
    public string Code { get; set; } = "";
    [Required, StringLength(300)]
    public string Title { get; set; } = "";
    [Range(1, int.MaxValue), Display(Name = "Teacher")]
    public int? TeacherId { get; set; }
}

public class EnrollmentInput
{
    [Range(1, int.MaxValue), Display(Name = "Student")]
    public int StudentId { get; set; }
    [Range(1, int.MaxValue), Display(Name = "Course")]
    public int CourseId { get; set; }
    [Required, DataType(DataType.Date), Display(Name = "Enrolled on")]
    public DateOnly? EnrolledOn { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}
