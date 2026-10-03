using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;

namespace UniversityWeb.Pages;

public class IndexModel(UniversityDb db) : PageModel
{
    public int Students { get; private set; }
    public int Teachers { get; private set; }
    public int Courses { get; private set; }
    public int Enrollments { get; private set; }

    public async Task OnGetAsync()
    {
        Students = await db.Students.CountAsync();
        Teachers = await db.Teachers.CountAsync();
        Courses = await db.Courses.CountAsync();
        Enrollments = await db.Enrollments.CountAsync();
    }
}
