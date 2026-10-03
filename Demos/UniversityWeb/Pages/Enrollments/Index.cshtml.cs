using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Enrollments;

public class IndexModel(UniversityDb db) : PageModel
{
    public List<EnrollmentRow> Rows { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Rows = await (from enrollment in db.Enrollments
                      join student in db.Students on enrollment.StudentId equals student.Id
                      join course in db.Courses on enrollment.CourseId equals course.Id
                      orderby student.FullName
                      select new EnrollmentRow(enrollment.Id, student.FullName, course.Code, enrollment.EnrolledOn)).ToListAsync();
    }
}
public record EnrollmentRow(int Id, string StudentName, string CourseCode, DateOnly EnrolledOn);
