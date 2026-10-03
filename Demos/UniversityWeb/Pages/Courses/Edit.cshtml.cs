using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Courses;

public class EditModel(UniversityDb db) : PageModel
{
    [BindProperty]
    public CourseInput Input { get; set; } = new();
    public List<Teacher> Teachers { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await db.Courses.FindAsync(id);
        if (item is null) return NotFound();
        Input.Code = item.Code;
        Input.Title = item.Title;
        Input.TeacherId = item.TeacherId;
        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var item = await db.Courses.FindAsync(id);
        if (item is null) return NotFound();
        await LoadOptionsAsync();
        if (!ModelState.IsValid) return Page();
        Input.Code = Input.Code.Trim();
        Input.Title = Input.Title.Trim();
        Input.Code = Input.Code.ToUpperInvariant();
        if (Input.TeacherId.HasValue && !await db.Teachers.AnyAsync(t => t.Id == Input.TeacherId))
            ModelState.AddModelError("Input.TeacherId", "Choose an existing teacher.");
        if (await db.Courses.AnyAsync(c => c.Id != id && c.Code == Input.Code))
            ModelState.AddModelError("Input.Code", "This course code already exists.");
        if (!ModelState.IsValid) return Page();

        item.Code = Input.Code;
        item.Title = Input.Title;
        item.TeacherId = Input.TeacherId;
        
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "The data changed or a duplicate exists. Check the fields and try again.");
            return Page();
        }
        TempData["Message"] = "Course updated.";
        return RedirectToPage("./Index");
    }
    
    private async Task LoadOptionsAsync()
    {
        Teachers = await db.Teachers.AsNoTracking().OrderBy(t => t.FullName).ToListAsync();
    }

}
