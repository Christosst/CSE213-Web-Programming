using Microsoft.AspNetCore.Mvc;
namespace CourseApi;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly TaskStore store;

    public TasksController(TaskStore store)
    {
        this.store = store;
    }

    [HttpGet]
    public ActionResult<TaskItem[]> GetAll([FromQuery] bool? completed)
    {
        return Ok(store.All(completed));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<TaskItem>(200)]
    [ProducesResponseType(404)]
    public ActionResult<TaskItem> Get(int id)
    {
        var task = store.Find(id);
        if (task is null) return NotFound();
        return Ok(task);
    }

    [HttpPost]
    [ProducesResponseType<TaskItem>(201)]
    [ProducesResponseType(400)]
    public ActionResult<TaskItem> Create(TaskWriteRequest request)
    {
        var task = store.Add(request);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult Update(int id, TaskWriteRequest request)
    {
        bool updated = store.Update(id, request);
        if (!updated) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public IActionResult Delete(int id)
    {
        bool deleted = store.Remove(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
