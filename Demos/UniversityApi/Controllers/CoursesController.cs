using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace UniversityApi;

[ApiController]
[Route("api/courses")]
public class CoursesController : UniversityControllerBase
{
    public CoursesController(UniversityDb db) : base(db) { }

    [HttpGet]
    public async Task<ActionResult<List<Course>>> GetAll()
    {
        return Ok(await db.Courses.AsNoTracking().OrderBy(item => item.Id).ToListAsync());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<Course>(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Course>> Get(int id)
    {
        var item = await db.Courses.FindAsync(id);
        if (item is null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType<Course>(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create(CourseRequest request)
    {
        var invalid = await Validate(request, 0);
        if (invalid is not null) return invalid;
        var item = new Course();
        item.Code = request.Code;
        item.Title = request.Title;
        item.TeacherId = request.TeacherId;
        db.Courses.Add(item);
        var conflict = await SaveChanges();
        if (conflict is not null) return conflict;
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Update(int id, CourseRequest request)
    {
        var item = await db.Courses.FindAsync(id);
        if (item is null) return NotFound();
        var invalid = await Validate(request, id);
        if (invalid is not null) return invalid;
        item.Code = request.Code;
        item.Title = request.Title;
        item.TeacherId = request.TeacherId;
        return await SaveChanges() ?? NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Courses.FindAsync(id);
        if (item is null) return NotFound();
        if (await db.Enrollments.AnyAsync(enrollment => enrollment.CourseId == id))
            return Problem(statusCode: 409, title: "Delete related enrollments first");
        db.Courses.Remove(item);
        return await SaveChanges() ?? NoContent();
    }

    private async Task<IActionResult?> Validate(CourseRequest request, int id)
    {
        request.Code = request.Code.Trim().ToUpperInvariant();
        request.Title = request.Title.Trim();
        if (!await db.Teachers.AnyAsync(teacher => teacher.Id == request.TeacherId))
            return Problem(statusCode: 400, title: "TeacherId does not identify a teacher");
        if (await db.Courses.AnyAsync(course => course.Code == request.Code && course.Id != id))
            return Problem(statusCode: 409, title: "Course code already exists");
        return null;
    }
}
