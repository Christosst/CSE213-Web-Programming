"""Exercise the University API against an isolated temporary SQLite database."""
from pathlib import Path
from contextlib import closing
import html
import json
import os
import sqlite3
import subprocess
import tempfile
import time
import urllib.error
import urllib.request

root = Path(__file__).resolve().parent.parent
project = root / 'Demos/UniversityApi'
subprocess.run(['dotnet', 'build', str(project)], check=True)
testFolder = root / '.test-data'
testFolder.mkdir(exist_ok=True)
checks = 0
base = 'http://localhost:5093/universityAPI'

def request(path, method='GET', data=None, expected=200):
    global checks
    req = urllib.request.Request(base + path,
        data=None if data is None else json.dumps(data).encode(),
        headers={'Content-Type': 'application/json'}, method=method)
    try:
        response = urllib.request.urlopen(req, timeout=5)
    except urllib.error.HTTPError as error:
        response = error
    with response:
        body = response.read()
        assert response.status == expected, (method, path, response.status, body)
        checks += 1
        value = json.loads(body) if body and 'json' in response.headers.get('Content-Type', '') else body.decode()
        return value, response.headers

with tempfile.TemporaryDirectory(prefix='university-test-', dir=testFolder) as temp:
    database = Path(temp) / 'university.db'
    env = os.environ.copy()
    env.update({'ASPNETCORE_ENVIRONMENT': 'Production', 'Swagger__Enabled': 'true',
        'ConnectionStrings__University': f'Data Source={database};Foreign Keys=True', 'PathBase': '/universityAPI'})

    def start():
        process = subprocess.Popen(['dotnet', str(project/'bin/Debug/net10.0/UniversityApi.dll'),
            '--urls', 'http://localhost:5093'], cwd=project, env=env,
            stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        for _ in range(100):
            try:
                urllib.request.urlopen(base+'/api/students', timeout=1).close()
                return process
            except OSError:
                if process.poll() is not None: raise RuntimeError('UniversityApi exited during startup')
                time.sleep(.1)
        process.terminate()
        raise RuntimeError('UniversityApi did not start')

    def stop(process):
        process.terminate()
        process.wait(timeout=15)

    process = start()
    try:
        for resource in ['students', 'teachers', 'courses', 'enrollments']:
            rows, _ = request('/api/'+resource)
            assert len(rows) == 1, 'Expected one seeded record per table'
            request('/api/'+resource+'/999999', expected=404)
            request('/api/'+resource+'/999999', 'DELETE', expected=404)
            request('/api/'+resource, 'POST', {}, expected=400)

        teacher, headers = request('/api/teachers', 'POST', {'fullName': '  Demo Teacher  ', 'email': 'demo.teacher@example.org'}, 201)
        assert teacher['fullName'] == 'Demo Teacher'
        assert '/universityAPI/api/teachers/' in headers['Location']
        student, _ = request('/api/students', 'POST', {'fullName': 'Demo Student', 'email': 'demo.student@example.org'}, 201)
        course, _ = request('/api/courses', 'POST', {'code': ' demo101 ', 'title': 'Demo Course', 'teacherId': teacher['id']}, 201)
        assert course['code'] == 'DEMO101'
        enrollmentBody = {'studentId': student['id'], 'courseId': course['id'], 'enrolledOn': '2026-10-02'}
        enrollment, _ = request('/api/enrollments', 'POST', enrollmentBody, 201)

        writes = {
            'teachers': (teacher, {'fullName': 'Updated Teacher', 'email': 'updated.teacher@example.org'}),
            'students': (student, {'fullName': 'Updated Student', 'email': 'updated.student@example.org'}),
            'courses': (course, {'code': 'DEMO102', 'title': 'Updated Course', 'teacherId': teacher['id']}),
            'enrollments': (enrollment, {**enrollmentBody, 'enrolledOn': '2026-10-03'})
        }
        for resource, (item, data) in writes.items():
            value, _ = request(f'/api/{resource}/{item["id"]}')
            assert value['id'] == item['id']
            empty, _ = request(f'/api/{resource}/{item["id"]}', 'PUT', data, 204)
            assert empty == ''
            changed, _ = request(f'/api/{resource}/{item["id"]}')
            for key, value in data.items(): assert changed[key] == value
            request(f'/api/{resource}/999999', 'PUT', data, 404)
            request('/api/'+resource, 'POST', data, 409)

        joined, _ = request(f'/api/students/{student["id"]}/courses')
        assert joined[0]['id'] == course['id'] and joined[0]['code'] == 'DEMO102'
        joined, _ = request(f'/api/courses/{course["id"]}/students')
        assert joined[0]['id'] == student['id']
        request('/api/students/999999/courses', expected=404)
        request('/api/courses/999999/students', expected=404)
        request('/api/courses', 'POST', {'code': 'BAD1', 'title': 'Unknown teacher', 'teacherId': 999999}, 400)
        request('/api/enrollments', 'POST', {**enrollmentBody, 'studentId': 999999}, 400)
        request('/api/enrollments', 'POST', {**enrollmentBody, 'courseId': 999999}, 400)
        request('/api/enrollments', 'POST', {**enrollmentBody, 'enrolledOn': '2026-02-30'}, 400)
        request('/api/students', 'POST', {'fullName': ' ', 'email': 'not-email'}, 400)
        for resource, item in [('teachers', teacher), ('students', student), ('courses', course)]:
            request(f'/api/{resource}/{item["id"]}', 'DELETE', expected=409)

        document, _ = request('/openapi/v1.json')
        operations = sum(len([method for method in methods if method in ['get','post','put','delete']]) for methods in document['paths'].values())
        assert operations == 22, operations
        assert any('/universityAPI' in server['url'] for server in document.get('servers', [])), document.get('servers')
        request('/swagger/index.html')
        request('/README.md')
        # Database file is outside wwwroot; a browser must not download it.
        request('/App_Data/university.db', expected=404)
    finally:
        stop(process)

    process = start()
    try:
        retained, _ = request(f'/api/enrollments/{enrollment["id"]}')
        assert retained['enrolledOn'] == '2026-10-03'
        for resource, item in [('enrollments', enrollment), ('students', student), ('courses', course), ('teachers', teacher)]:
            request(f'/api/{resource}/{item["id"]}', 'DELETE', expected=204)
            request(f'/api/{resource}/{item["id"]}', expected=404)
    finally:
        stop(process)
    process = start()
    try:
        rows, _ = request('/api/students')
        assert len(rows) == 1, 'Restart must not reinsert deleted test records or duplicate seed data'
    finally:
        stop(process)

    with closing(sqlite3.connect(database)) as connection:
        assert len(connection.execute('PRAGMA foreign_key_list(Enrollments)').fetchall()) == 2
        assert connection.execute("SELECT COUNT(*) FROM Teachers").fetchone()[0] == 1

print(f'Passed {checks} API checks: all CRUD methods, validation, relationships, conflicts, Swagger subpath and SQLite restart persistence.')
