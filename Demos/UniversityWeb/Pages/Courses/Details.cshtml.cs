using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Courses;

public class DetailsModel(UniversityDb db) : PageModel
{
    public string TeacherName { get; private set; } = "";
    public Course Item { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await db.Courses.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (item is null) return NotFound();
        Item = item;
        TeacherName = await db.Teachers.Where(t => t.Id == item.TeacherId).Select(t => t.FullName).FirstOrDefaultAsync() ?? "Not assigned";
        return Page();
    }
    
}
