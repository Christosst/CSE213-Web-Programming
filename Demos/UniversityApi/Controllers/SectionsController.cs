using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace UniversityApi;

[ApiController]
[Route("api/sections")]
public class SectionsController : UniversityControllerBase
{
    public SectionsController(UniversityDb db) : base(db) { }
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? courseId, [FromQuery] int? teacherId, [FromQuery] string? term)
    {
        var query = db.CourseSections.AsNoTracking().AsQueryable();
        if (courseId is not null) query = query.Where(section => section.CourseId == courseId);
        if (teacherId is not null) query = query.Where(section => section.TeacherId == teacherId);
        if (!string.IsNullOrWhiteSpace(term)) query = query.Where(section => section.Term == term);
        return Ok(await query.OrderBy(section => section.SectionCode).ToListAsync());
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await db.CourseSections.FindAsync(id);
        return item is null ? NotFound() : Ok(item);
    }
    [HttpPost]
    public async Task<IActionResult> Create(CourseSectionRequest request)
    {
        var invalid = await Validate(request, 0);
        if (invalid is not null) return invalid;
        var item = new CourseSection();
        item.SectionCode = request.SectionCode;
        item.SectionName = request.SectionName;
        item.CourseId = request.CourseId;
        item.TeacherId = request.TeacherId;
        item.Term = request.Term;
        item.Timing = request.Timing;
        item.Room = request.Room;
        item.ModeOfStudy = request.ModeOfStudy;
        item.ReportedEnrolledStudents = request.ReportedEnrolledStudents;
        item.ReportedNonRegisteredStudents = request.ReportedNonRegisteredStudents;
        item.Capacity = request.Capacity;
        item.School = request.School;
        item.Program = request.Program;
        item.Department = request.Department;
        item.Level = request.Level;
        item.Ects = request.Ects;
        item.TimetableInstructor = request.TimetableInstructor;

        db.CourseSections.Add(item);
        var conflict = await SaveChanges();
        return conflict ?? CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CourseSectionRequest request)
    {
        var item = await db.CourseSections.FindAsync(id);
        if (item is null) return NotFound();
        var invalid = await Validate(request, id);
        if (invalid is not null) return invalid;
        // Moving a populated section would invalidate its enrollments.
        if (item.CourseId != request.CourseId && await db.Enrollments.AnyAsync(e => e.CourseSectionId == id))
            return Problem(statusCode: 409, title: "Cannot move a section with enrollments to another course");
        item.SectionCode = request.SectionCode;
        item.SectionName = request.SectionName;
        item.CourseId = request.CourseId;
        item.TeacherId = request.TeacherId;
        item.Term = request.Term;
        item.Timing = request.Timing;
        item.Room = request.Room;
        item.ModeOfStudy = request.ModeOfStudy;
        item.ReportedEnrolledStudents = request.ReportedEnrolledStudents;
        item.ReportedNonRegisteredStudents = request.ReportedNonRegisteredStudents;
        item.Capacity = request.Capacity;
        item.School = request.School;
        item.Program = request.Program;
        item.Department = request.Department;
        item.Level = request.Level;
        item.Ects = request.Ects;
        item.TimetableInstructor = request.TimetableInstructor;

        return await SaveChanges() ?? NoContent();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.CourseSections.FindAsync(id);
        if (item is null) return NotFound();
        if (await db.Enrollments.AnyAsync(e => e.CourseSectionId == id))
            return Problem(statusCode: 409, title: "Delete related enrollments first");
        db.CourseSections.Remove(item);
        return await SaveChanges() ?? NoContent();
    }
    private async Task<IActionResult?> Validate(CourseSectionRequest request, int id)
    {
        request.SectionCode = request.SectionCode.Trim();
        request.SectionName = request.SectionName.Trim();
        request.Term = request.Term.Trim();
        if (!await db.Courses.AnyAsync(c => c.Id == request.CourseId))
            return Problem(statusCode: 400, title: "CourseId does not identify a course");
        if (request.TeacherId is not null && !await db.Teachers.AnyAsync(t => t.Id == request.TeacherId))
            return Problem(statusCode: 400, title: "TeacherId does not identify a teacher");
        if (await db.CourseSections.AnyAsync(s => s.SectionCode == request.SectionCode && s.Term == request.Term && s.Id != id))
            return Problem(statusCode: 409, title: "Section code already exists in this term");
        return null;
    }
}
