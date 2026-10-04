# CSE213 Web Programming — Bruno API Collection

This directory contains the complete [Bruno](https://www.usebruno.com/) collection for exploring and testing all **CSE213 Web Programming** course demos, REST Web APIs, and Razor Pages.

---

## Published Live URLs

The applications are published and accessible online:

| Application | Published URL | Description |
| :--- | :--- | :--- |
| **Course Catalogue & Demos** | [http://cse213.runasp.net/](http://cse213.runasp.net/) | Main course catalogue and HTML/CSS/JS demos |
| **CourseApi Swagger UI** | [http://cse213.runasp.net/swagger/index.html](http://cse213.runasp.net/swagger/index.html) | Interactive Swagger UI for CourseApi |
| **CourseApi OpenAPI Spec** | [http://cse213.runasp.net/openapi/v1.json](http://cse213.runasp.net/openapi/v1.json) | OpenAPI 3.0 document for CourseApi |
| **University API Swagger UI** | [http://cse213universityapi.runasp.net/swagger/index.html](http://cse213universityapi.runasp.net/swagger/index.html) | Interactive Swagger UI for UniversityApi |
| **University API Spec** | [http://cse213universityapi.runasp.net/openapi/v1.json](http://cse213universityapi.runasp.net/openapi/v1.json) | OpenAPI 3.0 document for UniversityApi |
| **University Razor Web** | [http://cse213universityweb.runasp.net/](http://cse213universityweb.runasp.net/) | ASP.NET Core Razor Pages university classroom |

---

## Environments in Bruno

You can switch environments at any time using the environment dropdown in the top-right corner of Bruno:

### 1. `Hosted` / `Production` (Default)
Tests the live deployed applications on Windows IIS hosting:
- `baseUrl`: `http://cse213.runasp.net`
- `apiUrl`: `http://cse213.runasp.net`
- `uniApiUrl`: `http://cse213universityapi.runasp.net`
- `uniWebUrl`: `http://cse213universityweb.runasp.net`
- `courseApiSwaggerUrl`: `http://cse213.runasp.net/swagger/index.html`
- `uniApiSwaggerUrl`: `http://cse213universityapi.runasp.net/swagger/index.html`

> **Note:** The collection defaults to the hosted URLs even when **No Environment** is selected, so requests work immediately out-of-the-box without requiring local server startup.

### 2. `Local`
Tests local development servers when running projects locally:
- `baseUrl`: `http://localhost:3000` (Local static web server)
- `apiUrl`: `http://localhost:5080` (CourseApi)
- `uniApiUrl`: `http://localhost:5082` (UniversityApi)
- `uniWebUrl`: `http://localhost:5083` (UniversityWeb)
- `courseApiSwaggerUrl`: `http://localhost:5080/swagger/index.html`
- `uniApiSwaggerUrl`: `http://localhost:5082/swagger/index.html`

---

## Collection Structure

```text
- Bruno Collection/
├── 00 - Demo Index.bru                # Main catalogue home page
├── 00 - CourseApi Swagger UI.bru      # CourseApi Swagger documentation
├── 00 - UniversityApi Swagger UI.bru  # UniversityApi Swagger documentation
├── 00 - University Razor Classroom.bru# University Razor Pages web app
├── 01 - Introduction/                 # Chapter 01-03 HTML demos
├── 02 - HTML/                         # Chapter 04-11 HTML demos
├── 03 - CSS/                          # Chapter 12-19 CSS layout & styling demos
├── 04 - JavaScript/                   # Chapter 20-25 DOM & JS demos
├── 05 - REST Web API/
│   ├── 26 - Hello.bru                 # Hello World JSON endpoint
│   ├── 27 - Swagger UI.bru            # CourseApi Swagger UI
│   ├── 27 - OpenAPI Spec.bru          # CourseApi OpenAPI JSON
│   ├── 27 - Read Tasks.bru            # Task store list
│   ├── 27 - Create Task.bru           # Create task
│   ├── 27 - Get Task by ID.bru        # Get single task
│   ├── 27 - Update Task.bru           # Update task
│   ├── 27 - Delete Task.bru           # Delete task
│   ├── 28 - Analyze Text.bru          # Text character & word count
│   ├── 28 - Uppercase Text.bru        # Text transformer
│   ├── 28 - Form Echo.bru             # HTML form submission echo
│   ├── 29 - API Landing Page.bru      # HTML frontend for CourseApi
│   ├── 29 - Tasks Demo Page.bru       # HTML frontend for Tasks
│   ├── 29 - Text Demo Page.bru        # HTML frontend for Text
│   └── University API/
│       ├── 01 - API Home.bru          # University API landing page
│       ├── 02 - Swagger UI.bru        # University API Swagger UI
│       ├── 02 - OpenAPI Spec.bru      # University API OpenAPI JSON definition
│       ├── 03 - Students - Get All.bru
│       ├── 04 - Students - Get by ID.bru
│       ├── 05 - Students - Create.bru
│       ├── 06 - Students - Update.bru
│       ├── 07 - Students - Delete.bru
│       ├── 08 - Teachers - Get All.bru
│       ├── 09 - Teachers - Get by ID.bru (Christos Stylianides)
│       ├── 10 - Teachers - Create.bru
│       ├── 11 - Teachers - Update.bru
│       ├── 12 - Teachers - Delete.bru
│       ├── 13 - Courses - Get All.bru
│       ├── 14 - Courses - Get by ID.bru (CSE213 Web Programming)
│       ├── 15 - Courses - Create.bru
│       ├── 16 - Courses - Update.bru
│       ├── 17 - Courses - Delete.bru
│       ├── 18 - Sections - Get All.bru
│       ├── 19 - Sections - Get by ID.bru (CSE213 - ECA section 46252)
│       ├── 20 - Sections - Create.bru
│       ├── 21 - Sections - Update.bru
│       ├── 22 - Sections - Delete.bru
│       ├── 23 - Enrollments - Get All.bru
│       ├── 24 - Enrollments - Get by ID.bru
│       ├── 25 - Enrollments - Create.bru
│       ├── 26 - Enrollments - Update.bru
│       ├── 27 - Enrollments - Delete.bru
│       ├── 28 - Query - Student Courses.bru
│       ├── 29 - Query - Course Students.bru (All 20 enrolled students in CSE213)
│       ├── 30 - Query - Teacher Courses.bru (Instructor sections)
│       ├── 31 - Query - Course Sections.bru (ECA, ECB, ECC sections)
│       └── 32 - Query - Section Students.bru (Students in section 46252)
├── 06 - Razor Pages/                  # Razor Pages classroom endpoints
├── 07 - Web Graphics and SVG/         # SVG and Canvas demos
└── environments/
    ├── Hosted.bru                     # Published cloud environment
    ├── Production.bru                 # Alias for Hosted
    └── Local.bru                      # Local development ports
```

---

## How to Open in Bruno

1. Download and open [Bruno](https://www.usebruno.com/).
2. Click **Open Collection**.
3. Select this folder: `CSE213 Web Programming/- Bruno Collection`.
4. In the top-right environment selector:
   - Select **Hosted** to run requests against the live published sites.
   - Select **Local** if you have started the projects on `localhost`.
5. Click **Send** on any request to inspect the response headers and JSON/HTML body.
