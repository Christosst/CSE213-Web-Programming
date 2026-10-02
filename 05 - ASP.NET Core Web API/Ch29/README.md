# Chapter 29: JavaScript and the API

Deck: CSE213-29-API-Frontend-Integration.pptx

Run instructions: [Demos/README.md](../../Demos/README.md). Start with `CourseApi/wwwroot/text.js`. Use one operation at a time.

## Instructor demonstration (15 minutes)

1. Open localhost:5080/text.html and submit Hello web; inspect fetch, JSON and the result.
2. Open tasks.html; add, complete and delete a task. Inspect POST, PUT and DELETE.
3. Read api.js: response.ok, automatic 400 details and skipping JSON for 204.
4. Throttle or block /api/** in DevTools; observe loading and recoverable error states.
5. Enter <img src=x> and show that the result is rendered as text.
6. Optional SQLite frontend: open localhost:5081 and repeat create/edit/delete.

## Student exercise (22 minutes)

Build a labelled form for your chosen text or SQLite endpoint with loading, safe output and visible failure.

## Scope

REST, controller-based ASP.NET Core on .NET 10, OpenAPI and Swagger UI. No MVC views, Razor, Blazor, authentication setup or repository/service layers. SQLite and its tiny EF Core example are optional. The slide deck's combined /api/notes demonstration is supplied as a separate app on port 5081 so the core text path has no EF dependency.
