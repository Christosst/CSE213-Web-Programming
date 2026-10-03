using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using UniversityApi;

var builder = WebApplication.CreateBuilder(args);
// UniversityApi initializes the database. Both apps open the same file.
var databasePath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath,
    "..", "UniversityApi", "App_Data", "university.db"));
var connection = new SqliteConnectionStringBuilder(
    builder.Configuration.GetConnectionString("University") ??
    $"Data Source={databasePath};Foreign Keys=True");
// Resolve configured relative paths from this app, including the hosted sibling API folder.
connection.DataSource = Path.GetFullPath(connection.DataSource, builder.Environment.ContentRootPath);
var connectionString = connection.ToString();
var sharedDatabase = connection.DataSource;
if (!File.Exists(sharedDatabase))
    throw new InvalidOperationException("Start UniversityApi first to create the shared university database. " +
        "For hosting, set ConnectionStrings__University to the existing API database file.");

builder.Services.AddRazorPages();
builder.Services.AddDbContext<UniversityDb>(options => options.UseSqlite(connectionString));

var app = builder.Build();
if (!string.IsNullOrWhiteSpace(builder.Configuration["PathBase"]))
    app.UsePathBase(builder.Configuration["PathBase"]);
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
