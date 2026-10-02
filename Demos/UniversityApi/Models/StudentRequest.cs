using System.ComponentModel.DataAnnotations;
namespace UniversityApi;

public class StudentRequest
{
    [Required, StringLength(200)]
    public string FullName { get; set; } = "";
    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }
    [StringLength(600)]
    public string? RegistrationNumber { get; set; }
    [StringLength(600)]
    public string? FirstName { get; set; }
    [StringLength(600)]
    public string? LastName { get; set; }
    [StringLength(600)]
    public string? Gender { get; set; }
    [StringLength(600)]
    public string? School { get; set; }
    [StringLength(600)]
    public string? Program { get; set; }
    [StringLength(600)]
    public string? Department { get; set; }
    [StringLength(600)]
    public string? Specialization { get; set; }
    [StringLength(600)]
    public string? AnnualResultsModel { get; set; }
    [StringLength(600)]
    public string? Curriculum { get; set; }
    [StringLength(600)]
    public string? ApplicationId { get; set; }
    public decimal? CumulativeGpa { get; set; }
}
