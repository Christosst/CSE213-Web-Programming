namespace UniversityApi;

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string? Email { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public string? School { get; set; }
    public string? Program { get; set; }
    public string? Department { get; set; }
    public string? Specialization { get; set; }
    public string? AnnualResultsModel { get; set; }
    public string? Curriculum { get; set; }
    public string? ApplicationId { get; set; }
    public decimal? CumulativeGpa { get; set; }
}
