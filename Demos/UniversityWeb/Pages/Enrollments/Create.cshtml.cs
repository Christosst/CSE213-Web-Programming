using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Enrollments;

public class CreateModel(UniversityDb db) : PageModel
{
    [BindProperty]
    public EnrollmentInput Input { get; set; } = new();
    public List<Student> Students { get; private set; } = [];
    public List<Course> Courses { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {

        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        
        await LoadOptionsAsync();
        if (!ModelState.IsValid) return Page();
        
        if (!await db.Students.AnyAsync(s => s.Id == Input.StudentId))
            ModelState.AddModelError("Input.StudentId", "Choose an existing student.");
        if (!await db.Courses.AnyAsync(c => c.Id == Input.CourseId))
            ModelState.AddModelError("Input.CourseId", "Choose an existing course.");
        if (await db.Enrollments.AnyAsync(e => e.Id != 0 && e.StudentId == Input.StudentId && e.CourseId == Input.CourseId))
            ModelState.AddModelError("", "This student is already enrolled in this course.");
        if (!ModelState.IsValid) return Page();

        var item = new Enrollment();
        item.StudentId = Input.StudentId;
        item.CourseId = Input.CourseId;
        item.EnrolledOn = Input.EnrolledOn!.Value;
        db.Enrollments.Add(item);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "The data changed or a duplicate exists. Check the fields and try again.");
            return Page();
        }
        TempData["Message"] = "Enrollment created.";
        return RedirectToPage("./Index");
    }
    
    private async Task LoadOptionsAsync()
    {
        Students = await db.Students.AsNoTracking().OrderBy(s => s.FullName).ToListAsync();
        Courses = await db.Courses.AsNoTracking().OrderBy(c => c.Code).ToListAsync();
    }

}
