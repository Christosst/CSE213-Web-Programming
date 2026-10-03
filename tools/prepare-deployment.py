"""Merge the course catalogue into an already-published CourseApi package."""
from pathlib import Path
import argparse
import json
import shutil

parser = argparse.ArgumentParser()
parser.add_argument('--publish-dir', required=True)
parser.add_argument('--swagger-enabled', choices=['true', 'false'], default='true')
parser.add_argument('--university-api-url', default='',
                    help='External base URL of the UniversityApi site, e.g. https://cse213universityapi.runasp.net')
parser.add_argument('--university-web-url', default='',
                    help='External base URL of the UniversityWeb site, e.g. https://cse213universityweb.runasp.net')
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

# Determine the external URLs (strip trailing slash).
uni_api_url = args.university_api_url.rstrip('/') if args.university_api_url else ''
uni_web_url = args.university_web_url.rstrip('/') if args.university_web_url else ''
# Fallback to relative paths when no external URL is provided (local/dev use).
uni_api_href = uni_api_url + '/' if uni_api_url else '/universityAPI/'
uni_web_href = uni_web_url + '/' if uni_web_url else '/universityWeb/'
uni_api_swagger_href = uni_api_url + '/swagger/' if uni_api_url else '/universityAPI/swagger/'

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
    # University API — now on its own site.
    text = text.replace('href="http://localhost:5082/"', f'href="{uni_api_href}"')
    text = text.replace('href="http://localhost:5082/', f'href="{uni_api_href}')
    # University Web — now on its own site.
    text = text.replace('href="http://localhost:5083/"', f'href="{uni_web_href}"')
    text = text.replace('href="http://localhost:5083/', f'href="{uni_web_href}')
    # Ch30 uses bare http://localhost:5083/ in body text (not an href) and has a
    # stale description about a shared database — fix both for the deployed version.
    if file.name == 'index.html' and file.parent.name == 'Ch30':
        text = text.replace(
            'http://localhost:5083/',
            uni_web_href)
        text = text.replace(
            'The deployment pipeline publishes it at <code>/universityWeb/</code>'
            ' with the same database as <code>/universityAPI/</code>.',
            f'The deployment pipeline publishes it at '
            f'<a href="{uni_web_href}">{uni_web_href}</a> '
            f'(its own hosted site, seeded from the same dataset as the API).')
        text = text.replace(
            'A Swagger change appears here after refreshing, and a Razor save appears in the API.',
            'Both apps are seeded from the same dataset; locally they share one database file.')
    if file == public / 'index.html':
        begin = text.index('    <p><strong>Start here:</strong>')
        end = text.index('</p>', begin) + len('</p>')
        text = text[:begin] + (f'    <p><strong>Hosted demos:</strong> the catalogue and REST API run on this website. '
            f'<a href="/api-demo.html">Open the API demos</a> · <a href="/swagger">Swagger UI</a> · '
            f'<a href="{uni_api_swagger_href}">University SQLite Swagger</a> · '
            f'<a href="{uni_web_href}">University Razor Pages</a> · '
            f'<a href="README.md">Local setup guide</a> · '
            f'<a href="COURSE-DEMO-REVIEW.md">Slide-to-demo review</a>.</p>') + text[end:]
        text = text.replace('Run CourseApi on port 5080', 'Use the API on this website')
        text = text.replace('Local .NET 10 project, port 5083', f'Hosted Razor Pages at {uni_web_href}')
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
# UniversityWeb is deployed to its own site: use a local App_Data/university.db.
# PathBase is removed because the app now runs at the root of its own domain.
web_settings = university_web / 'appsettings.Production.json'
web_configuration = json.loads(web_settings.read_text(encoding='utf-8')) if web_settings.exists() else {}
web_configuration.setdefault('ConnectionStrings', {})['University'] = (
    'Data Source=App_Data/university.db;Foreign Keys=True')
# Remove PathBase if previously set; the app is now at the domain root.
web_configuration.pop('PathBase', None)
web_settings.write_text(json.dumps(web_configuration, indent=2) + '\n', encoding='utf-8')
if list(university_web.rglob('*.db')):
    # university.db in App_Data is expected; a second database elsewhere is not.
    unexpected = [p for p in university_web.rglob('*.db') if p.parent.name != 'App_Data']
    if unexpected:
        raise SystemExit(f'Unexpected database file(s) in UniversityWeb: {unexpected}')
print('Prepared both APIs, UniversityWeb with local SQLite, and the public catalogue with hosted links.')

