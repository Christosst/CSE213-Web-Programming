using System.ComponentModel.DataAnnotations;
namespace UniversityApi;

public class CourseSectionRequest
{
    [Required, StringLength(200)]
    public string SectionCode { get; set; } = "";
    [Required, StringLength(200)]
    public string SectionName { get; set; } = "";
    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }
    [Range(1, int.MaxValue)]
    public int? TeacherId { get; set; }
    [Required, StringLength(200)]
    public string Term { get; set; } = "";
    [StringLength(600)]
    public string? Timing { get; set; }
    [StringLength(600)]
    public string? Room { get; set; }
    [StringLength(600)]
    public string? ModeOfStudy { get; set; }
    [Range(0, int.MaxValue)]
    public int? ReportedEnrolledStudents { get; set; }
    [Range(0, int.MaxValue)]
    public int? ReportedNonRegisteredStudents { get; set; }
    [Range(0, int.MaxValue)]
    public int? Capacity { get; set; }
    [StringLength(600)]
    public string? School { get; set; }
    [StringLength(600)]
    public string? Program { get; set; }
    [StringLength(600)]
    public string? Department { get; set; }
    [StringLength(600)]
    public string? Level { get; set; }
    public decimal? Ects { get; set; }
    [StringLength(600)]
    public string? TimetableInstructor { get; set; }
}
