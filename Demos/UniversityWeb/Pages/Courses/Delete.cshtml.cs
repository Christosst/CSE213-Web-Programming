using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Courses;

public class DeleteModel(UniversityDb db) : PageModel
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
    
    public async Task<IActionResult> OnPostAsync(int id)
    {
        var item = await db.Courses.FindAsync(id);
        if (item is null) return NotFound();
        Item = item;
        TeacherName = await db.Teachers.Where(t => t.Id == item.TeacherId).Select(t => t.FullName).FirstOrDefaultAsync() ?? "Not assigned";
        db.Courses.Remove(item);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Another record still uses this record. Remove its related records first.");
            return Page();
        }
        TempData["Message"] = "Course deleted.";
        return RedirectToPage("./Index");
    }

}
