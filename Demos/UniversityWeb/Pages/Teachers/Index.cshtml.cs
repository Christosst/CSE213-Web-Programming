using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Teachers;

public class IndexModel(UniversityDb db) : PageModel
{
    public List<Teacher> Rows { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Rows = await db.Teachers.AsNoTracking().OrderBy(item => item.FullName).ToListAsync();
    }
}
