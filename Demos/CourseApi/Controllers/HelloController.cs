using Microsoft.AspNetCore.Mvc;
namespace CourseApi;

[ApiController]
[Route("api/hello")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Hello from .NET 10" });
    }
}
