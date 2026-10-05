"""Read-only checks after deploying the course website."""
import argparse
import html
import json
import time
import urllib.error
import urllib.request

parser = argparse.ArgumentParser()
parser.add_argument('--url', required=True)
parser.add_argument('--university-api-url', default='',
                    help='External base URL of the UniversityApi site')
parser.add_argument('--university-web-url', default='',
                    help='External base URL of the UniversityWeb site')
parser.add_argument('--swagger-enabled', choices=['true', 'false'], default='true')
args = parser.parse_args()
base = args.url.rstrip('/')
uni_api_base = args.university_api_url.rstrip('/') if args.university_api_url else base + '/universityAPI'
uni_web_base = args.university_web_url.rstrip('/') if args.university_web_url else base + '/universityWeb'


def read(path, site_base=None):
    url = (site_base or base) + path
    with urllib.request.urlopen(url, timeout=20) as response:
        return response.read().decode('utf-8')


def read_api(path):
    return read(path, uni_api_base)


def read_web(path):
    return read(path, uni_web_base)


# Allow IIS time to start the newly deployed process; do not mutate classroom data.
for attempt in range(12):
    try:
        hello = json.loads(read('/api/hello'))
        assert hello.get('message') == 'Hello from .NET 10', 'Unexpected hello response'
        assert isinstance(json.loads(read('/api/tasks')), list), 'Tasks did not return an array'
        assert 'Course' in read('/') and 'Demo Library' in read('/'), 'Catalogue is unavailable'
        assert 'Text processing' in read('/text.html'), 'Text frontend is unavailable'
        students = json.loads(read_api('/api/students'))
        assert isinstance(students, list), 'University students are unavailable'
        assert isinstance(json.loads(read_api('/api/courses')), list), 'University courses are unavailable'
        assert isinstance(json.loads(read_api('/api/sections')), list), 'University sections are unavailable'
        web_home = read_web('/')
        assert '<h1>University classroom</h1>' in web_home, 'UniversityWeb is unavailable; configure its IIS application and shared database permissions'
        # When UniversityWeb is at the root the links should not contain /universityWeb prefix.
        assert 'href="/Students"' in web_home or 'href="Students"' in web_home, 'Razor links are missing'
        assert 'href="/css/site.css' in web_home or 'href="css/site.css' in web_home, 'Razor stylesheet link is missing'
        assert 'font-family' in read_web('/css/site.css'), 'UniversityWeb stylesheet is unavailable'
        web_students = read_web('/Students')
        assert '<h1>Students</h1>' in web_students, 'Razor student list is unavailable'
        if uni_api_base == uni_web_base:
            assert f'Students: {len(students)}' in web_home, 'Razor and API student counts do not match'
            if students:
                student = students[0]
                details = html.unescape(read_web(f'/Students/Details/{student["id"]}'))
                assert '<h1>Details student</h1>' in details and student['fullName'] in details, 'Razor and API student records do not match'
        else:
            assert 'Students: ' in web_home, 'Razor student count summary is missing'
            details = html.unescape(read_web('/Students/Details/1'))
            assert '<h1>Details student</h1>' in details, 'Razor student details page is unavailable'
        for entity in ['Teachers', 'Courses', 'Enrollments']:
            assert f'<h1>{entity}</h1>' in read_web('/' + entity), f'Razor {entity} list is unavailable'
        assert '__RequestVerificationToken' in read_web('/Students/Create'), 'Razor form is unavailable'
        if args.swagger_enabled == 'true':
            document = json.loads(read('/openapi/v1.json'))
            assert '/api/text/analysis' in document['paths'], 'Text API is absent from OpenAPI'
            assert 'Swagger' in read('/swagger/index.html'), 'Swagger UI is unavailable'
            university = json.loads(read_api('/openapi/v1.json'))
            assert '/api/enrollments' in university['paths'] and '/api/sections' in university['paths'], 'University CRUD APIs are absent'
            assert 'Swagger' in read_api('/swagger/index.html'), 'University Swagger UI is unavailable'
        print('Verified deployed catalogue, both APIs, Razor Pages/shared student data, frontend and configured Swagger.')
        break
    except (OSError, ValueError, AssertionError, urllib.error.URLError) as error:
        if attempt == 11:
            raise SystemExit(f'Deployment verification failed: {error}')
        time.sleep(5)


