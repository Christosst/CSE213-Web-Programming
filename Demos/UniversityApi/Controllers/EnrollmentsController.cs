using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace UniversityApi;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : UniversityControllerBase
{
    public EnrollmentsController(UniversityDb db) : base(db) { }

    [HttpGet]
    public async Task<ActionResult<List<Enrollment>>> GetAll()
    {
        return Ok(await db.Enrollments.AsNoTracking().OrderBy(item => item.Id).ToListAsync());
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<Enrollment>(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Enrollment>> Get(int id)
    {
        var item = await db.Enrollments.FindAsync(id);
        if (item is null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType<Enrollment>(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create(EnrollmentRequest request)
    {
        var invalid = await Validate(request, 0);
        if (invalid is not null) return invalid;
        var item = new Enrollment();
        item.StudentId = request.StudentId;
        item.CourseId = request.CourseId;
        item.CourseSectionId = request.CourseSectionId;
        item.EnrolledOn = request.EnrolledOn!.Value;
        db.Enrollments.Add(item);
        var conflict = await SaveChanges();
        if (conflict is not null) return conflict;
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Update(int id, EnrollmentRequest request)
    {
        var item = await db.Enrollments.FindAsync(id);
        if (item is null) return NotFound();
        var invalid = await Validate(request, id);
        if (invalid is not null) return invalid;
        item.StudentId = request.StudentId;
        item.CourseId = request.CourseId;
        item.CourseSectionId = request.CourseSectionId;
        item.EnrolledOn = request.EnrolledOn!.Value;
        return await SaveChanges() ?? NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Enrollments.FindAsync(id);
        if (item is null) return NotFound();
        
        db.Enrollments.Remove(item);
        return await SaveChanges() ?? NoContent();
    }

    private async Task<IActionResult?> Validate(EnrollmentRequest request, int id)
    {
        
        if (!await db.Students.AnyAsync(student => student.Id == request.StudentId))
            return Problem(statusCode: 400, title: "StudentId does not identify a student");
        if (!await db.Courses.AnyAsync(course => course.Id == request.CourseId))
            return Problem(statusCode: 400, title: "CourseId does not identify a course");
        if (await db.Enrollments.AnyAsync(enrollment => enrollment.StudentId == request.StudentId &&
            enrollment.CourseId == request.CourseId && enrollment.Id != id))
            return Problem(statusCode: 409, title: "Student is already enrolled in this course");
        if (request.CourseSectionId is not null && !await db.CourseSections.AnyAsync(section =>
            section.Id == request.CourseSectionId && section.CourseId == request.CourseId))
            return Problem(statusCode: 400, title: "CourseSectionId must identify a section of this course");
        return null;
    }
}
