using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace UniversityApi;

public abstract class UniversityControllerBase : ControllerBase
{
    protected readonly UniversityDb db;
    protected UniversityControllerBase(UniversityDb db) { this.db = db; }

    protected async Task<IActionResult?> SaveChanges()
    {
        try { await db.SaveChangesAsync(); return null; }
        catch (DbUpdateException error) when (error.InnerException is SqliteException sqlite && sqlite.SqliteErrorCode == 19)
        {
            return Problem(statusCode: 409, title: "Database relationship or unique-value conflict",
                detail: "Check duplicate values and related records, then retry.");
        }
    }
}
