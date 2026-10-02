using System.ComponentModel.DataAnnotations;
namespace UniversityApi;

public class StudentRequest
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = "";
}
