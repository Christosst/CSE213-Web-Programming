using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace UniversityApi;

[ApiController]
[Route("api")]
public class QueriesController : ControllerBase
{
    private readonly UniversityDb db;
    public QueriesController(UniversityDb db) { this.db = db; }

    [HttpGet("students/{id:int}/courses")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> StudentCourses(int id)
    {
        if (!await db.Students.AnyAsync(student => student.Id == id)) return NotFound();
        var result = await (from enrollment in db.Enrollments
            join course in db.Courses on enrollment.CourseId equals course.Id
            where enrollment.StudentId == id
            orderby course.Code
            select new { course.Id, course.Code, course.Title, enrollment.EnrolledOn }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("courses/{id:int}/students")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CourseStudents(int id)
    {
        if (!await db.Courses.AnyAsync(course => course.Id == id)) return NotFound();
        var result = await (from enrollment in db.Enrollments
            join student in db.Students on enrollment.StudentId equals student.Id
            where enrollment.CourseId == id
            orderby student.FullName
            select new { student.Id, student.FullName, enrollment.EnrolledOn }).ToListAsync();
        return Ok(result);
    }
}
