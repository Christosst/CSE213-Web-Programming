# Deploy the demo catalogue, REST APIs and Razor Pages

The Azure pipeline publishes **Demos/CourseApi**, **Demos/UniversityApi** and **Demos/UniversityWeb**, combines the API frontend with the course catalogue, saves a `course-website` artifact, then uploads the published package to the existing FTP/IIS destination. It runs read-only HTTP checks of both APIs, Razor Pages and their shared student data after deployment. Only a successful main-branch run deploys; pull-request runs do not deploy.

## Azure pipeline settings

- **ftpPassword** is defined in YAML using the existing FTP password. Confirm it is current.
- Check **ftpHost**, **ftpUser** and **remoteRoot**. Defaults preserve the previous FTP host/account and `wwwroot` directory.
- Check **siteUrl**, currently `http://cse213.runasp.net`. The verification step must reach the actual binding for that IIS site. Use HTTPS when configured by the host.
- **publishRuntime** defaults to `win-x86` for the shared host's 32-bit IIS pool. All three applications must use 32-bit pools. If you switch all pools to 64-bit, change this value to `win-x64` before publishing again.
- **swaggerEnabled** defaults to `true` for these classroom demos. Setting it to `false` disables hosted Swagger/OpenAPI; the course Swagger links then intentionally do not work.

The publish step includes the .NET 10 runtime (self-contained), an executable and the generated IIS `web.config`. The host still needs the ASP.NET Core IIS module/Hosting Bundle and a compatible application-pool architecture. Configure the website directory as an ASP.NET Core/IIS application in the hosting control panel. Keep its environment as Production; Swagger uses the explicit configuration switch instead of requiring Development mode.

## Deployed routes

| Address | Content |
| --- | --- |
| `/` | Course catalogue |
| `/api-demo.html` | API lesson landing page |
| `/api/hello` | Hello JSON |
| `/api/tasks` | Temporary task CRUD |
| `/api/text/analysis`, `/api/text/uppercase` | Text-processing POST endpoints |
| `/text.html`, `/tasks.html` | Same-origin JavaScript frontends |
| `/swagger`, `/openapi/v1.json` | Classroom Swagger and OpenAPI, when enabled |
| `/universityAPI/` | University SQLite landing page |
| `/universityAPI/swagger/`, `/universityAPI/openapi/v1.json` | University Swagger and its 30 operations |
| `/universityAPI/api/students`, `/universityAPI/api/teachers`, `/universityAPI/api/courses`, `/universityAPI/api/enrollments` | Persistent university CRUD |
| `/universityWeb/` | Razor Pages university website using the API's SQLite database |
| `/universityWeb/Students`, `/universityWeb/Teachers`, `/universityWeb/Courses`, `/universityWeb/Enrollments` | Browser CRUD pages with forms and validation |
| `/universityWeb/HowItWorks` | Razor Pages request-flow explanation |
| `/form-echo` | Chapter 9 form POST echo without storage |

The packaged chapter guides point to this website's API rather than a visitor's localhost. Source files retain their local teaching setup. Only public lesson files and selected teaching documents enter wwwroot; API source, secrets, Node dependencies, test output and database files are excluded.

## University IIS application: one-time setup

Configure **/universityAPI as a separate IIS application** in the hosting control panel, pointing to the published universityAPI folder. Use a **separate application pool** from the root CourseApi: both use ASP.NET Core in-process hosting. Match the published architecture. A virtual directory alone is insufficient.

Give the university application's pool write access to **App_Data**. The pipeline creates a populated App_Data/university.db from the bundled real dataset. Its separate database upload uses lftp --only-missing: an existing server database is skipped. If no database is present at startup, the app can create and import the initial dataset; an existing database is never reimported. The database is outside wwwroot and cannot be downloaded. The main website upload excludes App_Data, databases and journals. A separate --only-missing upload includes only university.db, never overwrites an existing database, and never uploads journals or backups. All three applications remain offline until those uploads complete. Back up the database before schema changes; the app includes a tested upgrade for the original four-table schema, backing it up before changes. Future unrelated schema changes need a deliberate upgrade.

Open /universityAPI/swagger/ and follow its README walkthrough. For 500/502 errors, check the child IIS application, separate pool, Hosting Bundle and directory permissions. The small SqliteNotesApi starter stays local on port 5081. Root CourseApi has no EF Core dependency; its temporary tasks reset on restart.

## Upload behaviour and recovery

### HTTP 500.32: Failed to load .NET Core host

This error commonly means the self-contained package and IIS worker process have different architectures. The previous pipeline published `win-x64`; a 32-bit pool cannot load that runtime in-process. The pipeline now publishes both APIs and UniversityWeb as `win-x86`. Confirm **Websites → Manage → Scripting → ASP.NET Bitness** is **32-bit** in the MonsterASP control panel for the root website and both university applications, then rerun the pipeline. Alternatively, keep `win-x64` and configure all three pools for 64-bit. Restart the applications after changing the host setting.

If the error persists after matching architectures, inspect the hosting control panel logs for the native host loading error. Retrying HTTP verification cannot repair a host architecture mismatch. See [Microsoft's 500.32 troubleshooting](https://learn.microsoft.com/en-us/aspnet/core/test/troubleshoot-azure-iis?view=aspnetcore-10.0#50032-ancm-failed-to-load-dll).

The pipeline uploads `app_offline.htm` to all three applications before replacing assemblies and removes all three after all files upload successfully. This stops both users of the shared SQLite file during deployment. There is a short maintenance window. If an upload fails, the maintenance pages stay in place to avoid running a partial package; rerun deployment to complete it. The FTP mirror does not delete unrelated remote files or databases. The saved artifact contains the full package for inspection or a manual redeployment.

The pipeline does not configure the hosting account or its IIS module remotely. Local publishing and package verification cannot confirm those server settings. No live deployment is triggered just by editing this file locally.

References: [Microsoft IIS hosting guidance](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-10.0), [.NET publishing](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish), [Azure UseDotNet task](https://learn.microsoft.com/en-us/azure/devops/pipelines/tasks/reference/use-dotnet-v2?view=azure-pipelines).

## Local verification

Run `python tools/test-university.py` with .NET 10 installed. It checks isolated SQLite CRUD, relationships, validation, conflicts, Swagger under /universityAPI and restart persistence. The Azure publish step runs this check before producing the deployment package.

## Before the first main-branch deployment

1. Confirm the `ftpPassword` value in YAML is current.
2. Create an IIS application with alias **universityAPI**, physical path **wwwroot/universityAPI**, and its own compatible application pool. Create the folder in the control panel if needed; the pipeline uploads the files.
3. Create another IIS application with alias **universityWeb**, physical path **wwwroot/universityWeb**, and its own compatible application pool. This is a one-time hosting-panel action; FTP cannot create IIS applications. A virtual directory alone is insufficient.
4. Grant **both university pools** read/write permission to **wwwroot/universityAPI/App_Data**, including creating SQLite journal files. The Razor app reads the API database directly.
5. Confirm the root website is also configured for ASP.NET Core and the host supports .NET 10 through the ASP.NET Core IIS module. Match the architecture of all three pools to `publishRuntime`.
6. Check in the source, including `Demos/UniversityWeb`, then run the main-branch pipeline. Its final step checks both APIs, Swagger, the Razor lists/forms/styles and matching student data.

Changing the URL does not move an existing database from an older university folder. If that older application has classroom data, stop it, back up its App_Data folder, and copy the database and any required journal files to the new application before starting it.

The full normalized roster/catalogue is committed under Demos/UniversityApi/SeedData/university-import.json and included outside wwwroot in the deployment package. Production config permits initial import only when creating a new database. The API and Swagger remain public. App_Data remains excluded from Git and the general upload. The artifact contains a freshly populated university.db, uploaded by a separate missing-only step. Existing databases, classroom edits, deletions, backups and journal files are preserved, even when the bundled dataset changes. Updating existing records requires an explicit instructor import; deploying code alone does not do it.

Database upload behaviour follows the [lftp mirror documentation](https://lftp.yar.ru/lftp-man.html). `--only-missing` skips files already present at the destination.


## UniversityWeb shared database

UniversityWeb is published as a self-contained .NET 10 IIS application at **/universityWeb/**, alongside the API at **/universityAPI/**. Locally it opens `Demos/UniversityApi/App_Data/university.db`. The API initializes a missing database; Razor Pages do not create or import a separate one.

The deployment preparation writes `universityWeb/appsettings.Production.json` with `ConnectionStrings:University` set to `Data Source=../universityAPI/App_Data/university.db;Foreign Keys=True` and `PathBase` set to `/universityWeb`. UniversityWeb resolves the database path from its own content root, so it works regardless of the IIS working directory. Both apps use **wwwroot/universityAPI/App_Data/university.db**; no second database is published for Razor Pages. If you have explicitly configured `ConnectionStrings__University` in the hosting panel, it overrides this file: set it to the same absolute SQLite file in both university apps.

Give both IIS pools access to the shared directory and preserve the database during deployments. Swagger saves appear in Razor Pages after refresh and Razor saves appear in the API. The package preparation rejects an accidental second Razor database. A 500 error on Razor startup can indicate a missing shared database, an incorrect connection-string override, or missing shared-directory permissions. A 404 at `/universityWeb/` usually means its IIS application has not been configured.
