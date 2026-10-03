"""Read-only checks after deploying the course website."""
import argparse
import html
import json
import time
import urllib.error
import urllib.request

parser = argparse.ArgumentParser()
parser.add_argument('--url', required=True)
parser.add_argument('--swagger-enabled', choices=['true', 'false'], default='true')
args = parser.parse_args()
base = args.url.rstrip('/')

def read(path):
    with urllib.request.urlopen(base + path, timeout=20) as response:
        return response.read().decode('utf-8')

# Allow IIS time to start the newly deployed process; do not mutate classroom data.
for attempt in range(12):
    try:
        hello = json.loads(read('/api/hello'))
        assert hello.get('message') == 'Hello from .NET 10', 'Unexpected hello response'
        assert isinstance(json.loads(read('/api/tasks')), list), 'Tasks did not return an array'
        assert 'Course' in read('/') and 'Demo Library' in read('/'), 'Catalogue is unavailable'
        assert 'Text processing' in read('/text.html'), 'Text frontend is unavailable'
        students = json.loads(read('/universityAPI/api/students'))
        assert isinstance(students, list), 'University students are unavailable'
        assert isinstance(json.loads(read('/universityAPI/api/courses')), list), 'University courses are unavailable'
        assert isinstance(json.loads(read('/universityAPI/api/sections')), list), 'University sections are unavailable'
        web_home = read('/universityWeb/')
        assert '<h1>University classroom</h1>' in web_home, 'UniversityWeb is unavailable; configure its IIS application and shared database permissions'
        assert 'href="/universityWeb/Students"' in web_home, 'Razor links are missing the application path'
        assert 'href="/universityWeb/css/site.css' in web_home, 'Razor stylesheet link is missing the application path'
        assert 'font-family' in read('/universityWeb/css/site.css'), 'UniversityWeb stylesheet is unavailable'
        web_students = read('/universityWeb/Students')
        assert '<h1>Students</h1>' in web_students, 'Razor student list is unavailable'
        assert f'Students: {len(students)}' in web_home, 'Razor and API student counts do not match'
        if students:
            student = students[0]
            details = html.unescape(read(f'/universityWeb/Students/Details/{student["id"]}'))
            assert '<h1>Details student</h1>' in details and student['fullName'] in details, 'Razor and API student records do not match'
        for entity in ['Teachers', 'Courses', 'Enrollments']:
            assert f'<h1>{entity}</h1>' in read('/universityWeb/' + entity), f'Razor {entity} list is unavailable'
        assert '__RequestVerificationToken' in read('/universityWeb/Students/Create'), 'Razor form is unavailable'
        if args.swagger_enabled == 'true':
            document = json.loads(read('/openapi/v1.json'))
            assert '/api/text/analysis' in document['paths'], 'Text API is absent from OpenAPI'
            assert 'Swagger' in read('/swagger/index.html'), 'Swagger UI is unavailable'
            university = json.loads(read('/universityAPI/openapi/v1.json'))
            assert '/api/enrollments' in university['paths'] and '/api/sections' in university['paths'], 'University CRUD APIs are absent'
            assert 'Swagger' in read('/universityAPI/swagger/index.html'), 'University Swagger UI is unavailable'
        print('Verified deployed catalogue, both APIs, Razor Pages/shared student data, frontend and configured Swagger.')
        break
    except (OSError, ValueError, AssertionError) as error:
        if attempt == 11:
            raise SystemExit(f'Deployment verification failed: {error}')
        time.sleep(5)
