# Chapter 27: Requests and Swagger

Deck: CSE213-27-API-Requests-Swagger.pptx

Run instructions: [Demos/README.md](../../Demos/README.md). Start with `CourseApi/Controllers/TasksController.cs`. Use one operation at a time.

## Instructor demonstration (15 minutes)

1. Open Swagger at localhost:5080/swagger and try GET /api/tasks.
2. POST {"title":"Read the slides","isCompleted":false}: expect 201, a body and Location.
3. Follow Location: expect 200. Try an unknown id: 404.
4. POST {} or a whitespace-only title: expect automatic 400 validation.
5. PUT a task, then DELETE it: both return 204 with no JSON body.

## Student exercise (22 minutes)

Record method, URL, request body, status, response body and Location for the CRUD sequence.

## Scope

REST, controller-based ASP.NET Core on .NET 10, OpenAPI and Swagger UI. No MVC views, Razor, Blazor, authentication setup or repository/service layers. SQLite and its EF Core example are demonstrated in UniversityApi on port 5082 so the core CourseApi text path has no EF dependency.
