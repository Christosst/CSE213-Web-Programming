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
            select new { course.Id, course.Code, course.Title, enrollment.CourseSectionId, enrollment.EnrolledOn }).ToListAsync();
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
            select new { student.Id, student.FullName, student.RegistrationNumber, enrollment.CourseSectionId, enrollment.EnrolledOn }).ToListAsync();
        return Ok(result);
    }

    [HttpGet("teachers/{id:int}/courses")]
    public async Task<IActionResult> TeacherCourses(int id)
    {
        if (!await db.Teachers.AnyAsync(t => t.Id == id)) return NotFound();
        return Ok(await (from section in db.CourseSections
            join course in db.Courses on section.CourseId equals course.Id
            where section.TeacherId == id
            orderby course.Code, section.SectionCode
            select new { CourseId = course.Id, course.Code, course.Title, SectionId = section.Id,
                section.SectionCode, section.SectionName, section.Term, section.Timing, section.Room }).ToListAsync());
    }
    [HttpGet("sections/{id:int}/students")]
    public async Task<IActionResult> SectionStudents(int id)
    {
        if (!await db.CourseSections.AnyAsync(s => s.Id == id)) return NotFound();
        return Ok(await (from enrollment in db.Enrollments
            join student in db.Students on enrollment.StudentId equals student.Id
            where enrollment.CourseSectionId == id
            orderby student.FullName
            select new { student.Id, student.FullName, student.RegistrationNumber, student.Program, enrollment.EnrolledOn }).ToListAsync());
    }
    [HttpGet("courses/{id:int}/sections")]
    public async Task<IActionResult> CourseSections(int id)
    {
        if (!await db.Courses.AnyAsync(c => c.Id == id)) return NotFound();
        return Ok(await db.CourseSections.AsNoTracking().Where(s => s.CourseId == id).OrderBy(s => s.SectionCode).ToListAsync());
    }
}
