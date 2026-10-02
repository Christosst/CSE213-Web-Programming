using Microsoft.AspNetCore.Mvc;
namespace CourseApi;

[ApiController]
[Route("api/text")]
public class TextController : ControllerBase
{
    [HttpPost("analysis")]
    public IActionResult Analyze(TextRequest request)
    {
        int characters = request.Text.Length;
        string[] words = request.Text.Split((char[]?)null,
            StringSplitOptions.RemoveEmptyEntries);
        return Ok(new { characters, words = words.Length });
    }

    [HttpPost("uppercase")]
    public IActionResult Uppercase(TextRequest request)
    {
        return Ok(new { text = request.Text.ToUpperInvariant() });
    }
}
