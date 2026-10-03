using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Enrollments;

public class DetailsModel(UniversityDb db) : PageModel
{
    public string StudentName { get; private set; } = "";
    public string CourseName { get; private set; } = "";
    public Enrollment Item { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await db.Enrollments.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (item is null) return NotFound();
        Item = item;
        StudentName = await db.Students.Where(s => s.Id == item.StudentId).Select(s => s.FullName).FirstAsync();
        CourseName = await db.Courses.Where(c => c.Id == item.CourseId).Select(c => c.Code + " — " + c.Title).FirstAsync();
        return Page();
    }
    
}
