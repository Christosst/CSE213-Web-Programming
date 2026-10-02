# Deploy the demo catalogue and REST API

The Azure pipeline now builds **Demos/CourseApi** and **Demos/UniversityApi**, combines its frontend with the course catalogue, saves a `course-website` artifact, then uploads the published package to the existing FTP/IIS destination. It runs a read-only HTTP check after deployment. Only a successful main-branch run deploys; pull-request runs do not deploy.

## Azure pipeline settings

- Define **ftpPassword** as a secret pipeline variable. Do not put the value in YAML.
- Check **ftpHost**, **ftpUser** and **remoteRoot**. Defaults preserve the previous FTP host/account and `wwwroot` directory.
- Check **siteUrl**, currently `http://cse213.runasp.net`. The verification step must reach the actual binding for that IIS site. Use HTTPS when configured by the host.
- **publishRuntime** defaults to `win-x64`. Set `win-x86` if the hosting application's pool runs in 32-bit mode.
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
| `/universityAPI/swagger/`, `/universityAPI/openapi/v1.json` | University Swagger and its 22 operations |
| `/universityAPI/api/students`, `/universityAPI/api/teachers`, `/universityAPI/api/courses`, `/universityAPI/api/enrollments` | Persistent university CRUD |
| `/form-echo` | Chapter 9 form POST echo without storage |

The packaged chapter guides point to this website's API rather than a visitor's localhost. Source files retain their local teaching setup. Only public lesson files and selected teaching documents enter wwwroot; API source, secrets, Node dependencies, test output and database files are excluded.

## University IIS application: one-time setup

Configure **/universityAPI as a separate IIS application** in the hosting control panel, pointing to the published universityAPI folder. Use a **separate application pool** from the root CourseApi: both use ASP.NET Core in-process hosting. Match the published architecture. A virtual directory alone is insufficient.

Give the university application's pool write access to **App_Data**. The app creates App_Data/university.db and seeds one example per entity only on database creation. The database is outside wwwroot and cannot be downloaded. The upload excludes App_Data, database files and journals and does not delete existing remote files, preserving records across deployments. Back up the database before schema changes; this small demo uses EnsureCreated, not migrations.

Open /universityAPI/swagger/ and follow its README walkthrough. For 500/502 errors, check the child IIS application, separate pool, Hosting Bundle and directory permissions. The small SqliteNotesApi starter stays local on port 5081. Root CourseApi has no EF Core dependency; its temporary tasks reset on restart.

## Upload behaviour and recovery

The pipeline uploads `app_offline.htm` to both applications before replacing assemblies and removes both after all files upload successfully. There is a short maintenance window. If an upload fails, the maintenance page stays in place to avoid running a partial package; rerun deployment to complete it. The FTP mirror does not delete unrelated remote files or databases. The saved artifact contains the full package for inspection or a manual redeployment.

The pipeline does not configure the hosting account or its IIS module remotely. Local publishing and package verification cannot confirm those server settings. No live deployment is triggered just by editing this file locally.

References: [Microsoft IIS hosting guidance](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-10.0), [.NET publishing](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-publish), [Azure UseDotNet task](https://learn.microsoft.com/en-us/azure/devops/pipelines/tasks/reference/use-dotnet-v2?view=azure-pipelines).

## Local verification

Run `python tools/test-university.py` with .NET 10 installed. It checks isolated SQLite CRUD, relationships, validation, conflicts, Swagger under /universityAPI and restart persistence. The Azure publish step runs this check before producing the deployment package.

## Before the first main-branch deployment

1. Create the `ftpPassword` secret in Azure Pipelines.
2. Create an IIS application with alias **universityAPI**, physical path **wwwroot/universityAPI**, and its own compatible application pool. Create the folder in the control panel if needed; the pipeline uploads the files.
3. Grant that pool write permission to **wwwroot/universityAPI/App_Data**.
4. Confirm the root website is also configured for ASP.NET Core and the host supports .NET 10 through the ASP.NET Core IIS module.
5. Check in the source, then run the main-branch pipeline. Its final step checks both APIs and Swagger.

Changing the URL does not move an existing database from an older university folder. If that older application has classroom data, stop it, back up its App_Data folder, and copy the database and any required journal files to the new application before starting it.
