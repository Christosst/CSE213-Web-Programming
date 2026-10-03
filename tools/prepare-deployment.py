"""Merge the course catalogue into an already-published CourseApi package."""
from pathlib import Path
import argparse
import json
import shutil

parser = argparse.ArgumentParser()
parser.add_argument('--publish-dir', required=True)
parser.add_argument('--swagger-enabled', choices=['true', 'false'], default='true')
args = parser.parse_args()
root = Path(__file__).resolve().parent.parent
publish = Path(args.publish_dir).resolve()
if publish == root or publish.is_relative_to(root / 'Demos'):
    raise SystemExit('Use a separate artifact directory, outside the API source projects.')
if not (publish / 'CourseApi.dll').is_file():
    raise SystemExit('Publish CourseApi before preparing the website.')
university = publish / 'universityAPI'
if not (university / 'UniversityApi.dll').is_file():
    raise SystemExit('Publish UniversityApi to the universityAPI subfolder before preparing the website.')
university_web = publish / 'universityWeb'
if not (university_web / 'UniversityWeb.dll').is_file():
    raise SystemExit('Publish UniversityWeb to the universityWeb subfolder before preparing the website.')
public = publish / 'wwwroot'
public.mkdir(exist_ok=True)
# Keep the original API landing page, while the site root becomes the catalogue.
shutil.copy2(root / 'Demos/CourseApi/wwwroot/index.html', public / 'api-demo.html')
for name in ['assets', '01 - Introduction', '02 - HTML', '03 - CSS',
             '04 - JavaScript', '05 - ASP.NET Core Web API',
             '06 - ASP.NET Core Razor Pages', 'Optional']:
    shutil.copytree(root / name, public / name, dirs_exist_ok=True)
for name in ['index.html', 'README.md', 'COURSE-DEMO-REVIEW.md', 'DEPLOYMENT.md']:
    shutil.copy2(root / name, public / name)
# Only teaching documents are public. Backend source, configuration and data stay private.
(public / 'Demos').mkdir(exist_ok=True)
for name in ['README.md', 'CSharp-Quick-Reference.md']:
    shutil.copy2(root / 'Demos' / name, public / 'Demos' / name)
(public / 'Demos/UniversityWeb').mkdir(exist_ok=True)
shutil.copy2(root / 'Demos/UniversityWeb/README.md', public / 'Demos/UniversityWeb/README.md')

for file in public.rglob('*.html'):
    text = file.read_text(encoding='utf-8-sig')
    text = text.replace('href="http://localhost:5080/"', 'href="/api-demo.html"')
    text = text.replace('href="http://localhost:5080/', 'href="/')
    # The optional database app remains a local exercise; don't link a visitor's localhost.
    text = text.replace('http://localhost:5081', 'the optional local notes app (port 5081)')
    text = text.replace('href="http://localhost:5082/"', 'href="/universityAPI/"')
    text = text.replace('href="http://localhost:5082/', 'href="/universityAPI/')
    text = text.replace('href="http://localhost:5083/', 'href="/universityWeb/')
    if file == public / 'index.html':
        begin = text.index('    <p><strong>Start here:</strong>')
        end = text.index('</p>', begin) + len('</p>')
        text = text[:begin] + ('    <p><strong>Hosted demos:</strong> the catalogue and REST API run on this website. '
            '<a href="/api-demo.html">Open the API demos</a> Â· <a href="/swagger">Swagger UI</a> Â· '
            '<a href="/universityAPI/swagger/">University SQLite Swagger</a> Â· '
            '<a href="/universityWeb/">University Razor Pages</a> · '
            '<a href="README.md">Local setup guide</a> · '
            '<a href="COURSE-DEMO-REVIEW.md">Slide-to-demo review</a>.</p>') + text[end:]
        text = text.replace('Run CourseApi on port 5080', 'Use the API on this website')
        text = text.replace('Local .NET 10 project, port 5083', 'Hosted Razor Pages at /universityWeb/')
    elif file.parent.parent.name == '05 - ASP.NET Core Web API':
        text = text.replace('Start the .NET 10 project in a separate terminal. These links open its local server; static HTML alone cannot run C#.',
            'The core API and university SQLite API are hosted on this website. Use the links below. The commands show how to run local copies; the small notes starter remains local.')
    elif file.name in ['text.html', 'tasks.html']:
        text = text.replace('href="/">API home', 'href="/api-demo.html">API home')
    file.write_text(text, encoding='utf-8')

# Swagger is deliberately enabled for the hosted teaching application, independently of Development.
settings = publish / 'appsettings.Production.json'
configuration = json.loads(settings.read_text(encoding='utf-8')) if settings.exists() else {}
configuration.setdefault('Swagger', {})['Enabled'] = args.swagger_enabled == 'true'
settings.write_text(json.dumps(configuration, indent=2) + '\n', encoding='utf-8')
if not (university / 'SeedData/university-import.json').is_file():
    raise SystemExit('The full university seed dataset was not published.')
universitySettings = university / 'appsettings.Production.json'
universitySettings.write_text(json.dumps({'Swagger': {'Enabled': args.swagger_enabled == 'true'}, 'SeedData': {'ImportPath': 'SeedData/university-import.json'}}, indent=2) + '\n', encoding='utf-8')
# One shared database: the API owns initialization and the missing-only FTP upload.
# UniversityWeb resolves this sibling path from its content root, not the IIS working directory.
web_settings = university_web / 'appsettings.Production.json'
web_configuration = json.loads(web_settings.read_text(encoding='utf-8')) if web_settings.exists() else {}
web_configuration.setdefault('ConnectionStrings', {})['University'] = (
    'Data Source=../universityAPI/App_Data/university.db;Foreign Keys=True')
web_configuration['PathBase'] = '/universityWeb'
web_settings.write_text(json.dumps(web_configuration, indent=2) + '\n', encoding='utf-8')
if list(university_web.rglob('*.db')):
    raise SystemExit('UniversityWeb must share the API database; do not publish a second database.')
print('Prepared both APIs, UniversityWeb with shared SQLite, and the public catalogue with hosted links.')
