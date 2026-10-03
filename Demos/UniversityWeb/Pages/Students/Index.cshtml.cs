using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Students;

public class IndexModel(UniversityDb db) : PageModel
{
    public List<Student> Students { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Students = await db.Students.AsNoTracking().OrderBy(item => item.FullName).ToListAsync();
    }
}
