using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
namespace CourseApi;

// Supports the chapter 9 HTML form when the demo catalogue is hosted by this app.
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("form-echo")]
public class FormEchoController : ControllerBase
{
    [HttpPost]
    [Consumes("application/x-www-form-urlencoded")]
    [RequestSizeLimit(16384)]
    public async Task<IActionResult> Post()
    {
        var form = await Request.ReadFormAsync();
        var fields = form.ToDictionary(field => field.Key, field => field.Value.ToString());
        var json = JsonSerializer.Serialize(fields, new JsonSerializerOptions { WriteIndented = true });
        var page = "<!doctype html><html lang=\"en\"><head><meta charset=\"utf-8\">" +
            "<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">" +
            "<title>Form received</title></head><body><main><h1>Form received</h1>" +
            "<p>This classroom endpoint echoes data without saving it.</p><pre>" +
            WebUtility.HtmlEncode(json) + "</pre><a href=\"/\">Back to demos</a></main></body></html>";
        return Content(page, "text/html; charset=utf-8");
    }
}
