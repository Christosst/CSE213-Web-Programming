using System.ComponentModel.DataAnnotations;
namespace CourseApi;

public class TaskWriteRequest
{
    [Required, StringLength(120)]
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }
}
