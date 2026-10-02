using System.ComponentModel.DataAnnotations;
namespace UniversityApi;

public class CourseRequest
{
    [Required, StringLength(20)]
    public string Code { get; set; } = "";
    [Required, StringLength(150)]
    public string Title { get; set; } = "";
    [Range(1, int.MaxValue)]
    public int TeacherId { get; set; }
}
