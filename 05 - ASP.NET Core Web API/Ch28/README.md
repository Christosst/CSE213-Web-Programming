# Chapter 28: CRUD, text and optional SQLite

Deck: CSE213-28-API-CRUD-Text-Storage.pptx

Run instructions: [Demos/README.md](../../Demos/README.md). Start with `CourseApi/Controllers/TextController.cs`. Use one operation at a time.

## Instructor demonstration (15 minutes)

1. Test POST /api/text/analysis with {"text":"Hello web"}: characters 9, words 2.
2. Test uppercase: HELLO WEB. Test blank, missing and oversized input: 400.
3. Create a temporary task, restart CourseApi, and show that it disappears.
4. Optional SQLite: start SqliteNotesApi on port 5081, create/update/delete a note in Swagger, then verify a retained note after restart.

## Student exercise (22 minutes)

Choose text: add POST /api/text/trim. Or choose SQLite: complete four CRUD operations and prove persistence.

## Scope

REST, controller-based ASP.NET Core on .NET 10, OpenAPI and Swagger UI. No MVC views, Razor, Blazor, authentication setup or repository/service layers. SQLite and its tiny EF Core example are optional. The slide deck's combined /api/notes demonstration is supplied as a separate app on port 5081 so the core text path has no EF dependency.
