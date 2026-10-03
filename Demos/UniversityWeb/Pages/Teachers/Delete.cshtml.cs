using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Teachers;

public class DeleteModel(UniversityDb db) : PageModel
{
    public Teacher Item { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await db.Teachers.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (item is null) return NotFound();
        Item = item;
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(int id)
    {
        var item = await db.Teachers.FindAsync(id);
        if (item is null) return NotFound();
        Item = item;
        db.Teachers.Remove(item);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Another record still uses this record. Remove its related records first.");
            return Page();
        }
        TempData["Message"] = "Teacher deleted.";
        return RedirectToPage("./Index");
    }

}
