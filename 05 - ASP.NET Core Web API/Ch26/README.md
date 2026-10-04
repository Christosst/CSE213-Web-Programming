# Chapter 26: REST and .NET 10

Deck: CSE213-26-API-REST-DotNet10.pptx

Run instructions: [Demos/README.md](../../Demos/README.md). Start with `CourseApi/Controllers/HelloController.cs`. Use one operation at a time.

## Instructor demonstration (15 minutes)

1. Run CourseApi; open /api/hello and explain its JSON response.
2. Read Program.cs service registration, Build, middleware and MapControllers.
3. Read HelloController: route, HttpGet and Ok.

## Student exercise (22 minutes)

Add GET /api/course returning a course name and code.

## Scope

REST, controller-based ASP.NET Core on .NET 10, OpenAPI and Swagger UI. No MVC views, Razor, Blazor, authentication setup or repository/service layers. SQLite and its EF Core example are demonstrated in UniversityApi on port 5082 so the core CourseApi text path has no EF dependency.
