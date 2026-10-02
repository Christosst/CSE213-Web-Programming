using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace UniversityApi;

[ApiController]
[Route("api/teachers")]
public class TeachersController : UniversityControllerBase
{
    public TeachersController(UniversityDb db) : base(db) { }

    [HttpGet]
    public async Task<ActionResult<List<Teacher>>> GetAll([FromQuery] string? search, [FromQuery] string? email)
    {
        var query = db.Teachers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(t => t.FullName.Contains(search));
        if (!string.IsNullOrWhiteSpace(email)) { email = email.Trim().ToLowerInvariant(); query = query.Where(t => t.Email == email); }
        return Ok(await query.OrderBy(t => t.Id).ToListAsync());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<Teacher>(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Teacher>> Get(int id)
    {
        var item = await db.Teachers.FindAsync(id);
        if (item is null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType<Teacher>(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create(TeacherRequest request)
    {
        var invalid = await Validate(request, 0);
        if (invalid is not null) return invalid;
        var item = new Teacher();
        item.FullName = request.FullName;
        item.Email = request.Email;
        item.WorkdayId = request.WorkdayId;
        item.EmploymentType = request.EmploymentType;

        db.Teachers.Add(item);
        var conflict = await SaveChanges();
        if (conflict is not null) return conflict;
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Update(int id, TeacherRequest request)
    {
        var item = await db.Teachers.FindAsync(id);
        if (item is null) return NotFound();
        var invalid = await Validate(request, id);
        if (invalid is not null) return invalid;
        item.FullName = request.FullName;
        item.Email = request.Email;
        item.WorkdayId = request.WorkdayId;
        item.EmploymentType = request.EmploymentType;

        return await SaveChanges() ?? NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Teachers.FindAsync(id);
        if (item is null) return NotFound();
        if (await db.Courses.AnyAsync(course => course.TeacherId == id) ||
            await db.CourseSections.AnyAsync(section => section.TeacherId == id))
            return Problem(statusCode: 409, title: "Delete related courses and sections first");
        db.Teachers.Remove(item);
        return await SaveChanges() ?? NoContent();
    }

    private async Task<IActionResult?> Validate(TeacherRequest request, int id)
    {
        request.FullName = request.FullName.Trim();
        request.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        if (request.Email is not null && await db.Teachers.AnyAsync(item => item.Email == request.Email && item.Id != id))
            return Problem(statusCode: 409, title: "Email already exists");
        return null;
    }
}
