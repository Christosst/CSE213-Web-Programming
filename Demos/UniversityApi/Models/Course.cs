namespace UniversityApi;

public class Course
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public int? TeacherId { get; set; }
    public string? School { get; set; }
    public string? Program { get; set; }
    public string? Department { get; set; }
    public string? Level { get; set; }
    public decimal? Ects { get; set; }
}
