using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Teachers;

public class CreateModel(UniversityDb db) : PageModel
{
    [BindProperty]
    public TeacherInput Input { get; set; } = new();

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {

        if (!ModelState.IsValid) return Page();
        Input.FullName = Input.FullName.Trim();
        Input.Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim();
        if (Input.Email != null && await db.Teachers.AnyAsync(t => t.Id != 0 && t.Email == Input.Email))
            ModelState.AddModelError("Input.Email", "This email already exists.");
        if (!ModelState.IsValid) return Page();

        var item = new Teacher();
        item.FullName = Input.FullName;
        item.Email = Input.Email;
        db.Teachers.Add(item);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "The data changed or a duplicate exists. Check the fields and try again.");
            return Page();
        }
        TempData["Message"] = "Teacher created.";
        return RedirectToPage("./Index");
    }
    
}
