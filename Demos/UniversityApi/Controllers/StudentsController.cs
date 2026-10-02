using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace UniversityApi;

[ApiController]
[Route("api/students")]
public class StudentsController : UniversityControllerBase
{
    public StudentsController(UniversityDb db) : base(db) { }

    [HttpGet]
    public async Task<ActionResult<List<Student>>> GetAll([FromQuery] string? search, [FromQuery] string? program)
    {
        var query = db.Students.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(s => s.FullName.Contains(search) || (s.RegistrationNumber != null && s.RegistrationNumber.Contains(search)));
        if (!string.IsNullOrWhiteSpace(program)) query = query.Where(s => s.Program == program);
        return Ok(await query.OrderBy(s => s.Id).ToListAsync());
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
        item.RegistrationNumber = request.RegistrationNumber;
        item.FirstName = request.FirstName;
        item.LastName = request.LastName;
        item.Gender = request.Gender;
        item.School = request.School;
        item.Program = request.Program;
        item.Department = request.Department;
        item.Specialization = request.Specialization;
        item.AnnualResultsModel = request.AnnualResultsModel;
        item.Curriculum = request.Curriculum;
        item.ApplicationId = request.ApplicationId;
        item.CumulativeGpa = request.CumulativeGpa;

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
        item.RegistrationNumber = request.RegistrationNumber;
        item.FirstName = request.FirstName;
        item.LastName = request.LastName;
        item.Gender = request.Gender;
        item.School = request.School;
        item.Program = request.Program;
        item.Department = request.Department;
        item.Specialization = request.Specialization;
        item.AnnualResultsModel = request.AnnualResultsModel;
        item.Curriculum = request.Curriculum;
        item.ApplicationId = request.ApplicationId;
        item.CumulativeGpa = request.CumulativeGpa;

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
        request.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        if (request.Email is not null && await db.Students.AnyAsync(item => item.Email == request.Email && item.Id != id))
            return Problem(statusCode: 409, title: "Email already exists");
        request.RegistrationNumber = string.IsNullOrWhiteSpace(request.RegistrationNumber) ? null : request.RegistrationNumber.Trim();
        if (request.RegistrationNumber is not null && await db.Students.AnyAsync(item => item.RegistrationNumber == request.RegistrationNumber && item.Id != id))
            return Problem(statusCode: 409, title: "Registration number already exists");
        return null;
    }
}
