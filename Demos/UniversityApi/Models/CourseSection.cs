namespace UniversityApi;

public class CourseSection
{
    public int Id { get; set; }
    public string SectionCode { get; set; } = "";
    public string SectionName { get; set; } = "";
    public int CourseId { get; set; }
    public int? TeacherId { get; set; }
    public string Term { get; set; } = "";
    public string? Timing { get; set; }
    public string? Room { get; set; }
    public string? ModeOfStudy { get; set; }
    public int? ReportedEnrolledStudents { get; set; }
    public int? ReportedNonRegisteredStudents { get; set; }
    public int? Capacity { get; set; }
    public string? School { get; set; }
    public string? Program { get; set; }
    public string? Department { get; set; }
    public string? Level { get; set; }
    public decimal? Ects { get; set; }
    public string? TimetableInstructor { get; set; }
}
