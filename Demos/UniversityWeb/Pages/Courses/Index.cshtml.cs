using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Courses;

public class IndexModel(UniversityDb db) : PageModel
{
    public List<CourseRow> Rows { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Rows = await (from course in db.Courses
                      join teacher in db.Teachers on course.TeacherId equals teacher.Id into teachers
                      from teacher in teachers.DefaultIfEmpty()
                      orderby course.Code
                      select new CourseRow(course.Id, course.Code, course.Title, teacher == null ? "Not assigned" : teacher.FullName)).ToListAsync();
    }
}
public record CourseRow(int Id, string Code, string Title, string TeacherName);
