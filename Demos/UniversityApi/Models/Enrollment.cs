namespace UniversityApi;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateOnly EnrolledOn { get; set; }
    public int? CourseSectionId { get; set; }
}
