# Just enough C# for the API lessons

Use this as a short bridge from JavaScript. Read HelloController first, then the request model and TasksController. There is no separate full C# course inside this module.

| Familiar JavaScript idea | C# example | Meaning |
| --- | --- | --- |
| String value | `string title = "Read HTTP";` | A typed text variable |
| Number value | `int id = 1;` | An integer id |
| Boolean value | `bool completed = false;` | True/false |
| Inferred variable | `var task = store.Find(id);` | Compiler infers the type; it is still statically typed |
| Object data | `new TaskItem { Id = 1, Title = "Read HTTP" }` | An instance of a class with properties |
| Missing value | `TaskItem? task` and `task is null` | The reference may be absent |
| Function | `public IActionResult Get() { return Ok(...); }` | An accessible controller method returning an HTTP result |
| Await a Promise | `await db.SaveChangesAsync();` | Wait for asynchronous work; used in the optional database app |

`public class` defines a type. `get; set;` defines a property. `namespace` groups related types. Square brackets such as `[HttpGet]` are attributes: ASP.NET reads them to discover routes and validation rules.

The TasksController constructor receives TaskStore. `AddSingleton<TaskStore>()` tells ASP.NET to provide the same shared store throughout this application's lifetime. This is dependency injection; students only need to follow the supplied registration and constructor pattern.

`Ok` produces 200, `CreatedAtAction` produces 201 and a Location, `NoContent` produces 204, and `NotFound` produces 404. `[ApiController]` checks the request model and automatically returns 400 for invalid input.

Start with explicit, short methods. Explain the class/property/method distinction before changing an endpoint. The storage lock and LINQ expressions are supplied support code; the learning focus is the request and response contract.
