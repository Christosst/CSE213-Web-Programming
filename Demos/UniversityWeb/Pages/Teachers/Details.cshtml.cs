using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Teachers;

public class DetailsModel(UniversityDb db) : PageModel
{
    public Teacher Item { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await db.Teachers.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (item is null) return NotFound();
        Item = item;
        return Page();
    }
    
}
