using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace UniversityApi;

[ApiController]
[Route("api/students")]
public class StudentsController : UniversityControllerBase
{
    public StudentsController(UniversityDb db) : base(db) { }

    [HttpGet]
    public async Task<ActionResult<List<Student>>> GetAll()
    {
        return Ok(await db.Students.AsNoTracking().OrderBy(item => item.Id).ToListAsync());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<Student>(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Student>> Get(int id)
    {
        var item = await db.Students.FindAsync(id);
        if (item is null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType<Student>(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create(StudentRequest request)
    {
        var invalid = await Validate(request, 0);
        if (invalid is not null) return invalid;
        var item = new Student();
        item.FullName = request.FullName;
        item.Email = request.Email;
        db.Students.Add(item);
        var conflict = await SaveChanges();
        if (conflict is not null) return conflict;
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Update(int id, StudentRequest request)
    {
        var item = await db.Students.FindAsync(id);
        if (item is null) return NotFound();
        var invalid = await Validate(request, id);
        if (invalid is not null) return invalid;
        item.FullName = request.FullName;
        item.Email = request.Email;
        return await SaveChanges() ?? NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Students.FindAsync(id);
        if (item is null) return NotFound();
        if (await db.Enrollments.AnyAsync(enrollment => enrollment.StudentId == id))
            return Problem(statusCode: 409, title: "Delete related enrollments first");
        db.Students.Remove(item);
        return await SaveChanges() ?? NoContent();
    }

    private async Task<IActionResult?> Validate(StudentRequest request, int id)
    {
        request.FullName = request.FullName.Trim();
        request.Email = request.Email.Trim().ToLowerInvariant();
        if (await db.Students.AnyAsync(item => item.Email == request.Email && item.Id != id))
            return Problem(statusCode: 409, title: "Email already exists");
        return null;
    }
}
