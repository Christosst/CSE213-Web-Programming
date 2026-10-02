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
public = publish / 'wwwroot'
public.mkdir(exist_ok=True)
# Keep the original API landing page, while the site root becomes the catalogue.
shutil.copy2(root / 'Demos/CourseApi/wwwroot/index.html', public / 'api-demo.html')
for name in ['assets', '01 - Introduction', '02 - HTML', '03 - CSS',
             '04 - JavaScript', '05 - ASP.NET Core Web API', 'Optional']:
    shutil.copytree(root / name, public / name, dirs_exist_ok=True)
for name in ['index.html', 'README.md', 'COURSE-DEMO-REVIEW.md', 'DEPLOYMENT.md']:
    shutil.copy2(root / name, public / name)
# Only teaching documents are public. Backend source, configuration and data stay private.
(public / 'Demos').mkdir(exist_ok=True)
for name in ['README.md', 'CSharp-Quick-Reference.md']:
    shutil.copy2(root / 'Demos' / name, public / 'Demos' / name)

for file in public.rglob('*.html'):
    text = file.read_text(encoding='utf-8-sig')
    text = text.replace('href="http://localhost:5080/"', 'href="/api-demo.html"')
    text = text.replace('href="http://localhost:5080/', 'href="/')
    # The optional database app remains a local exercise; don't link a visitor's localhost.
    text = text.replace('http://localhost:5081', 'the optional local notes app (port 5081)')
    text = text.replace('href="http://localhost:5082/"', 'href="/universityAPI/"')
    text = text.replace('href="http://localhost:5082/', 'href="/universityAPI/')
    if file == public / 'index.html':
        begin = text.index('    <p><strong>Start here:</strong>')
        end = text.index('</p>', begin) + len('</p>')
        text = text[:begin] + ('    <p><strong>Hosted demos:</strong> the catalogue and REST API run on this website. '
            '<a href="/api-demo.html">Open the API demos</a> · <a href="/swagger">Swagger UI</a> · '
            '<a href="/universityAPI/swagger/">University SQLite Swagger</a> · '
            '<a href="README.md">Local setup guide</a> · '
            '<a href="COURSE-DEMO-REVIEW.md">Slide-to-demo review</a>.</p>') + text[end:]
        text = text.replace('Run CourseApi on port 5080', 'Use the API on this website')
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
universitySettings = university / 'appsettings.Production.json'
universitySettings.write_text(json.dumps({'Swagger': {'Enabled': args.swagger_enabled == 'true'}}, indent=2) + '\n', encoding='utf-8')
print('Prepared CourseApi, separate UniversityApi and public catalogue with hosted API links.')
