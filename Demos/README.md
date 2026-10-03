# .NET 10 backend demos: REST Web API and Razor Pages

Prerequisite: .NET 10 SDK (`dotnet --version` should start with 10). NuGet restore needs internet the first time. Run these commands from the repository root.

## Core path: hello, tasks and text (no EF Core)

```sh
dotnet run --project Demos/CourseApi --launch-profile classroom
```

Open http://localhost:5080/ and http://localhost:5080/swagger. The app serves both frontend and API from this origin. Stop it with Ctrl+C. `dotnet build Demos/CourseApi` checks compilation. Swagger/OpenAPI run in Development, which the classroom profile sets, or when the explicit Swagger:Enabled setting is true for the hosted classroom app. See [deployment instructions](../DEPLOYMENT.md).

If students have not used C#, first spend about 15–20 minutes with [CSharp-Quick-Reference.md](CSharp-Quick-Reference.md).

Read in this order:

1. Chapter 26: Program.cs and Controllers/HelloController.cs. Skip the task/text files until the next lesson.
2. Chapter 27: Models/TaskWriteRequest.cs and Controllers/TasksController.cs. Use Swagger before the frontend.
3. Chapter 28: TaskStore.cs and Controllers/TextController.cs. Temporary tasks reset on restart; text processing stores nothing.
4. Chapter 29: wwwroot/text.js, then api.js and tasks.js. Follow one request through to safe DOM output.
5. Additional chapter 30: [UniversityWeb](UniversityWeb/README.md), a three-hour Razor Pages introduction using the same SQLite database and entity definitions as UniversityApi. Start the API first. Run `dotnet run --project Demos/UniversityWeb --launch-profile classroom` and open http://localhost:5083/. Use browser pages for these CRUD operations; the REST API continues to use Swagger.

| Request | Expected result |
| --- | --- |
| GET /api/hello | 200 with message |
| GET /api/tasks?completed=false | 200 filtered array; initially empty |
| POST /api/tasks with title and isCompleted | 201, server-assigned id and Location |
| GET /api/tasks/{id} | 200 or 404 |
| PUT /api/tasks/{id} | 204 or 404 |
| DELETE /api/tasks/{id} | 204 or 404 |
| POST /api/text/analysis with {"text":"Hello web"} | 200: characters 9, words 2 |
| POST /api/text/uppercase with {"text":"Hello web"} | 200: text HELLO WEB |
| Missing, whitespace-only or over-limit text/title | 400 validation problem details |

Task titles allow 1–120 characters; text allows 1–5000. [Required] rejects blank/whitespace strings. The analysis character count uses .NET string.Length (UTF-16 code units, so some emoji count as two). Words split on whitespace, including tabs/newlines; punctuation stays within a word. Uppercase uses ToUpperInvariant. Define these rules before testing.

## Optional assignment path: SQLite notes with minimal EF Core

In another terminal:

```sh
dotnet run --project Demos/SqliteNotesApi --launch-profile classroom
```

Open http://localhost:5081/ and http://localhost:5081/swagger. GET/POST /api/notes and GET/PUT/DELETE /api/notes/{id} follow the same status-code pattern as tasks. Note text allows 1–500 characters. The app creates notes.db in its project directory and keeps records after restart. Test persistence before deleting the note. Delete the classroom database only when you intend to reset its data.

Only this optional project references EF Core. Teach the model, one DbContext, AddDbContext/UseSqlite, queries and SaveChangesAsync. EnsureCreated is a new-database classroom shortcut; migrations, relationships and repository patterns are outside scope. Schema edits after a database has been created are not applied automatically.

**Difference from the slide demo:** notes run on port 5081 in a separate small app rather than /api/notes in CourseApi. Each app serves its own frontend, so neither requires CORS. This keeps the text assignment completely free of EF Core. The chapter guide pages explain this difference.

## Assignment choices

- Text service: at least two operations, explicit processing rules, server validation, Swagger evidence, and a JavaScript form with loading/error/result states.
- SQLite CRUD: at least four HTTP operations, server validation, Swagger evidence, JavaScript create/edit/delete, and records proven to survive restart.

No deployment, authentication or complex architecture is required for these teaching demos. The localhost HTTP profile is intended for classroom use.

Official references: [OpenAPI in ASP.NET Core 10](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0), [API controllers and automatic validation](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-10.0).

## University SQLite extension

The fuller SQLite classroom demo is [UniversityApi](UniversityApi/README.md): students, teachers, courses, sections and enrollments, with an ER diagram and 30 Swagger operations. Start it on port 5082. It is included in the deployment package at /universityAPI; the simple notes starter on port 5081 remains available locally.

[UniversityWeb](UniversityWeb/README.md) is also included at `/universityWeb/`, with browser CRUD pages that use the same deployed SQLite file as UniversityApi. The hosting panel must configure this folder as a separate IIS application and grant its pool access to the API's `App_Data` directory. The pipeline uploads a database only when missing and takes both university apps offline while updating them.

The local UniversityApi database now contains the supplied 20-student roster and F2026 course/teacher catalogue. All students are assigned to Christos Stylianides' CSE213 ECA section. The real normalized dataset is included in Git and the deployment package under UniversityApi/SeedData, and is used to build the first-deployment SQLite database. The pipeline skips an existing hosted database, preserving all edits. SQLite files remain in ignored App_Data. See UniversityApi/README.md for refresh instructions and source mapping.
