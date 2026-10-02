using System.ComponentModel.DataAnnotations;
namespace UniversityApi;

public class CourseRequest
{
    [Required, StringLength(50)]
    public string Code { get; set; } = "";
    [Required, StringLength(300)]
    public string Title { get; set; } = "";
    [Range(1, int.MaxValue)]
    public int? TeacherId { get; set; }
    [StringLength(600)]
    public string? School { get; set; }
    [StringLength(600)]
    public string? Program { get; set; }
    [StringLength(600)]
    public string? Department { get; set; }
    [StringLength(600)]
    public string? Level { get; set; }
    public decimal? Ects { get; set; }
}
