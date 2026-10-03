using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Teachers;

public class EditModel(UniversityDb db) : PageModel
{
    [BindProperty]
    public TeacherInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await db.Teachers.FindAsync(id);
        if (item is null) return NotFound();
        Input.FullName = item.FullName;
        Input.Email = item.Email;
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var item = await db.Teachers.FindAsync(id);
        if (item is null) return NotFound();
        
        if (!ModelState.IsValid) return Page();
        Input.FullName = Input.FullName.Trim();
        Input.Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim();
        if (Input.Email != null && await db.Teachers.AnyAsync(t => t.Id != id && t.Email == Input.Email))
            ModelState.AddModelError("Input.Email", "This email already exists.");
        if (!ModelState.IsValid) return Page();

        item.FullName = Input.FullName;
        item.Email = Input.Email;
        
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "The data changed or a duplicate exists. Check the fields and try again.");
            return Page();
        }
        TempData["Message"] = "Teacher updated.";
        return RedirectToPage("./Index");
    }
    
}
