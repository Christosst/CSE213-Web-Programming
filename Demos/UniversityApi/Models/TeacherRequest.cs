using System.ComponentModel.DataAnnotations;
namespace UniversityApi;

public class TeacherRequest
{
    [Required, StringLength(200)]
    public string FullName { get; set; } = "";
    [EmailAddress, StringLength(150)]
    public string? Email { get; set; }
    [StringLength(600)]
    public string? WorkdayId { get; set; }
    [StringLength(600)]
    public string? EmploymentType { get; set; }
}
