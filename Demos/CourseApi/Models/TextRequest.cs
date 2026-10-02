using System.ComponentModel.DataAnnotations;
namespace CourseApi;

public class TextRequest
{
    [Required, StringLength(5000)]
    public string Text { get; set; } = "";
}
