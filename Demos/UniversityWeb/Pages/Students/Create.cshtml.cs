using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UniversityApi;
using UniversityWeb.Models;

namespace UniversityWeb.Pages.Students;

public class CreateModel(UniversityDb db) : PageModel
{
    [BindProperty]
    public StudentInput Input { get; set; } = new();

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {

        if (!ModelState.IsValid) return Page();
        Input.FullName = Input.FullName.Trim();
        Input.RegistrationNumber = Input.RegistrationNumber.Trim();
        Input.Email = string.IsNullOrWhiteSpace(Input.Email) ? null : Input.Email.Trim();
        if (await db.Students.AnyAsync(s => s.Id != 0 && s.RegistrationNumber == Input.RegistrationNumber))
            ModelState.AddModelError("Input.RegistrationNumber", "This registration number already exists.");
        if (Input.Email != null && await db.Students.AnyAsync(s => s.Id != 0 && s.Email == Input.Email))
            ModelState.AddModelError("Input.Email", "This email already exists.");
        if (!ModelState.IsValid) return Page();

        var item = new Student();
        item.FullName = Input.FullName;
        item.RegistrationNumber = Input.RegistrationNumber;
        item.Email = Input.Email;
        db.Students.Add(item);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "The data changed or a duplicate exists. Check the fields and try again.");
            return Page();
        }
        TempData["Message"] = "Student created.";
        return RedirectToPage("./Index");
    }
    
}
