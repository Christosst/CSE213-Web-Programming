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
        for resource in ['students', 'teachers', 'courses', 'enrollments', 'sections']:
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
        sectionBody = {'sectionCode':'DEMO-TEST', 'sectionName':'Demo section', 'courseId':course['id'], 'teacherId':teacher['id'], 'term':'F2026'}
        section, _ = request('/api/sections', 'POST', sectionBody, 201)
        enrollmentBody = {'studentId': student['id'], 'courseId': course['id'], 'enrolledOn': '2026-10-02', 'courseSectionId':section['id']}
        enrollment, _ = request('/api/enrollments', 'POST', enrollmentBody, 201)

        writes = {
            'sections': (section, {**sectionBody, 'sectionName':'Updated section'}),
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

        request(f'/api/sections/{section['id']}', 'DELETE', expected=409)
        roster, _ = request(f'/api/sections/{section['id']}/students')
        assert roster[0]['id'] == student['id']
        teaching, _ = request(f'/api/teachers/{teacher['id']}/courses')
        assert teaching[0]['sectionId'] == section['id']
        sections, _ = request(f'/api/courses/{course['id']}/sections')
        assert sections[0]['id'] == section['id']
        request('/api/teachers/999999/courses', expected=404)
        request('/api/sections/999999/students', expected=404)
        request('/api/courses/999999/sections', expected=404)
        request(f'/api/enrollments/{enrollment['id']}', 'PUT', {**enrollmentBody, 'courseSectionId':1}, 400)
        request(f'/api/sections/{section['id']}', 'PUT', {**sectionBody, 'courseId':1}, 409)
        noEmail, _ = request('/api/students', 'POST', {'fullName':'No Email', 'registrationNumber':'TEST001'}, 201)
        assert noEmail['email'] is None
        request('/api/students', 'POST', {'fullName':'Duplicate Number', 'registrationNumber':'TEST001'}, 409)
        request(f'/api/students/{noEmail['id']}', 'DELETE', expected=204)
        document, _ = request('/openapi/v1.json')
        operations = sum(len([method for method in methods if method in ['get','post','put','delete']]) for methods in document['paths'].values())
        assert operations == 30, operations
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
        for resource, item in [('enrollments', enrollment), ('sections', section), ('students', student), ('courses', course), ('teachers', teacher)]:
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
        assert len(connection.execute('PRAGMA foreign_key_list(Enrollments)').fetchall()) == 3
        assert connection.execute("SELECT COUNT(*) FROM Teachers").fetchone()[0] == 1

# Preserve records and relationships when upgrading the original four-table database.
with tempfile.TemporaryDirectory(prefix='university-upgrade-', dir=testFolder) as temp:
    database = Path(temp) / 'legacy.db'
    with closing(sqlite3.connect(database)) as connection:
        connection.executescript("""
        CREATE TABLE Students (Id INTEGER PRIMARY KEY AUTOINCREMENT, FullName TEXT NOT NULL, Email TEXT NOT NULL);
        CREATE TABLE Teachers (Id INTEGER PRIMARY KEY AUTOINCREMENT, FullName TEXT NOT NULL, Email TEXT NOT NULL);
        CREATE TABLE Courses (Id INTEGER PRIMARY KEY AUTOINCREMENT, Code TEXT NOT NULL, Title TEXT NOT NULL, TeacherId INTEGER NOT NULL REFERENCES Teachers(Id) ON DELETE RESTRICT);
        CREATE TABLE Enrollments (Id INTEGER PRIMARY KEY AUTOINCREMENT, StudentId INTEGER NOT NULL REFERENCES Students(Id) ON DELETE RESTRICT, CourseId INTEGER NOT NULL REFERENCES Courses(Id) ON DELETE RESTRICT, EnrolledOn TEXT NOT NULL);
        CREATE UNIQUE INDEX IX_Students_Email ON Students(Email);
        CREATE UNIQUE INDEX IX_Teachers_Email ON Teachers(Email);
        CREATE UNIQUE INDEX IX_Courses_Code ON Courses(Code);
        CREATE UNIQUE INDEX IX_Enrollments_StudentId_CourseId ON Enrollments(StudentId,CourseId);
        INSERT INTO Students VALUES(1,'Legacy Student','legacy.student@example.org');
        INSERT INTO Teachers VALUES(1,'Legacy Teacher','legacy.teacher@example.org');
        INSERT INTO Courses VALUES(1,'LEG101','Legacy Course',1);
        INSERT INTO Enrollments VALUES(1,1,1,'2026-10-01');
        """)
        connection.commit()
    env['ConnectionStrings__University'] = f'Data Source={database};Foreign Keys=True'
    process = start()
    try:
        legacy, _ = request('/api/students/1')
        assert legacy['fullName'] == 'Legacy Student' and legacy['registrationNumber'] is None
        preserved, _ = request('/api/enrollments/1')
        assert preserved['studentId'] == 1 and preserved['courseId'] == 1
        sections, _ = request('/api/sections')
        assert sections == []
        created, _ = request('/api/students', 'POST', {'fullName':'Imported without email','registrationNumber':'UPGRADE01'}, 201)
        assert created['email'] is None
    finally:
        stop(process)
    assert len(list(Path(temp).glob('*.v1-backup-*.db'))) == 1
    with closing(sqlite3.connect(database)) as connection:
        assert connection.execute('PRAGMA foreign_key_check').fetchall() == []
        assert len(connection.execute('PRAGMA foreign_key_list(Enrollments)').fetchall()) == 3
    process = start()
    try:
        preserved, _ = request('/api/students/1')
        assert preserved['fullName'] == 'Legacy Student'
    finally:
        stop(process)
    assert len(list(Path(temp).glob('*.v1-backup-*.db'))) == 1

# A new database loads the bundled roster; an existing one is never reimported.
with tempfile.TemporaryDirectory(prefix='university-bundle-', dir=testFolder) as temp:
    database = Path(temp) / 'bundled.db'
    bundle = Path(temp) / 'university-import.json'
    source = json.loads((project/'SeedData/university-import.json').read_text(encoding='utf-8'))
    bundle.write_text(json.dumps(source), encoding='utf-8')
    env['ConnectionStrings__University'] = f'Data Source={database};Foreign Keys=True'
    env['SeedData__ImportPath'] = str(bundle)
    process = start()
    try:
        for resource, expected in [('students',20),('teachers',714),('courses',1002),('sections',2324),('enrollments',20)]:
            rows, _ = request('/api/'+resource)
            assert len(rows) == expected, (resource,len(rows))
        students, _ = request('/api/students')
        removed = students[0]
        enrollments, _ = request('/api/enrollments')
        enrollment = next(e for e in enrollments if e['studentId'] == removed['id'])
        request(f'/api/enrollments/{enrollment["id"]}', 'DELETE', expected=204)
        request(f'/api/students/{removed["id"]}', 'DELETE', expected=204)
    finally:
        stop(process)
    source['Students'][0]['FullName'] = 'Changed source must not overwrite the database'
    bundle.write_text(json.dumps(source), encoding='utf-8')
    process = start()
    try:
        students, _ = request('/api/students')
        assert len(students) == 19, 'Changed bundle must preserve deletions in an existing database'
        assert not any(s['fullName'] == source['Students'][0]['FullName'] for s in students)
        enrollments, _ = request('/api/enrollments')
        assert len(enrollments) == 19
    finally:
        stop(process)
    assert not list(Path(temp).glob('*.before-import-*.db'))
    with closing(sqlite3.connect(database)) as connection:
        assert connection.execute('PRAGMA foreign_key_check').fetchall() == []

print(f'Passed {checks} API checks: all CRUD methods, validation, relationships, conflicts, Swagger subpath and SQLite restart persistence.')
