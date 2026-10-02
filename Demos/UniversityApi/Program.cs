using UniversityApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);
var dataFolder = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataFolder);
var databasePath = Path.Combine(dataFolder, "university.db");
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<UniversityDb>(options => options.UseSqlite(
    builder.Configuration.GetConnectionString("University") ??
    $"Data Source={databasePath};Foreign Keys=True"));

var app = builder.Build();
// IIS supplies PathBase automatically. This optional setting also permits a local subpath check.
if (!string.IsNullOrWhiteSpace(builder.Configuration["PathBase"]))
    app.UsePathBase(builder.Configuration["PathBase"]);
app.UseExceptionHandler();
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.MapOpenApi();
    // Relative document URL works both locally and under the IIS /universityAPI application.
    app.UseSwaggerUI(options => options.SwaggerEndpoint("../openapi/v1.json", "University API v1"));
}
app.UseDefaultFiles();
var contentTypes = new FileExtensionContentTypeProvider();
contentTypes.Mappings[".md"] = "text/plain; charset=utf-8";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = contentTypes });
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UniversityDb>();
    // Seed only a newly-created database, not every server restart.
    if (db.Database.EnsureCreated()) DemoData.Seed(db);
}
app.Run();
