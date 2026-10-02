"""Read the supplied CSV/XLSX into a private, repeatable university import.

Requires openpyxl. This file never writes source records into wwwroot.
"""
import argparse
import csv
import json
from pathlib import Path
import openpyxl

parser = argparse.ArgumentParser()
parser.add_argument('--deployment-bundle', action='store_true', help='Explicitly prepare the public Git/deployment snapshot')
parser.add_argument('--students', required=True)
parser.add_argument('--courses', required=True)
parser.add_argument('--output', default='Demos/UniversityApi/App_Data/imports/university-import.json')
args = parser.parse_args()

def text(value):
    if value is None:
        return None
    if isinstance(value, float) and value.is_integer():
        value = int(value)
    result = str(value).strip()
    return None if result in ['', '-'] else result

def number(value):
    value = text(value)
    if value is None:
        return None
    try:
        return float(value.replace(',', '.'))
    except ValueError:
        return None

with Path(args.students).open(encoding='utf-8-sig', newline='') as source:
    roster = list(csv.DictReader(source))
workbook = openpyxl.load_workbook(args.courses, read_only=True, data_only=True)
iterator = iter(workbook.active.values)
headers = next(iterator)
rows = [dict(zip(headers, row)) for row in iterator if row[0]]
workbook.close()

students = []
for row in roster:
    fields = {'RegistrationNumber': 'Registration Number', 'FirstName': 'First Name',
              'LastName': 'Last Name', 'Gender': 'Gender', 'School': 'Business Unit|School',
              'Program': 'Program', 'Department': 'Study Area|Department',
              'Specialization': 'Specialization', 'AnnualResultsModel': 'Annual Results Model',
              'Curriculum': 'Curriculum', 'ApplicationId': 'Application ID'}
    item = {key: text(row.get(column)) for key, column in fields.items()}
    item['FullName'] = f"{item['FirstName']} {item['LastName']}"
    item['Email'] = None  # The supplied student CSV contains no email column.
    item['CumulativeGpa'] = number(row.get('Cumulative GPA'))
    students.append(item)
assert len({s['RegistrationNumber'] for s in students}) == len(students)
assert all(s['RegistrationNumber'] and s['FirstName'] and s['LastName'] for s in students)

# Prefer email as a teacher identity. Resolve an email-less occurrence by its known name.
known_names = {}
for row in rows:
    name, email = text(row['Instructor (Section)']), text(row['Instructor Email (Section)'])
    if name and email:
        known_names.setdefault(name.casefold(), email.lower())
teachers, courses, sections = {}, {}, []
for row in rows:
    name = text(row['Instructor (Section)'])
    email = text(row['Instructor Email (Section)'])
    email = email.lower() if email else known_names.get(name.casefold()) if name else None
    workday = text(row['Workday ID (Section)'])
    teacher_key = (email or f"name:{name.casefold()}") if name else None
    if teacher_key:
        item = teachers.setdefault(teacher_key, {'SourceKey': teacher_key, 'FullName': name,
            'Email': email, 'WorkdayId': workday, 'EmploymentType': text(row['Instructor (Employment Type)'])})
        if not item['WorkdayId'] and workday:
            item['WorkdayId'] = workday
    code = text(row['Course Description']).upper()
    abbreviation = text(row['Course Abbreviation'])
    title = abbreviation.split('-', 1)[1].strip() if '-' in abbreviation else abbreviation
    item = courses.setdefault(code, {'Code': code, 'Title': title, 'TeacherKey': teacher_key,
        'School': text(row['School']), 'Program': text(row['Program']),
        'Department': text(row['Study Area Department']), 'Level': text(row['Level']),
        'Ects': number(row['ECTS'])})
    if item['TeacherKey'] is None and teacher_key:
        item['TeacherKey'] = teacher_key
    sections.append({'SectionCode': text(row['Section Code']), 'SectionName': text(row['Section']),
        'CourseCode': code, 'TeacherKey': teacher_key, 'Term': 'F2026',
        'Timing': text(row['Timing']), 'Room': text(row['Room']),
        'ModeOfStudy': text(row['Mode Of Study (Course Section Type)']),
        'School': text(row['School']), 'Program': text(row['Program']),
        'Department': text(row['Study Area Department']), 'Level': text(row['Level']),
        'Ects': number(row['ECTS']), 'TimetableInstructor': text(row['Instructor (Timetable)']),
        'ReportedEnrolledStudents': int(number(row['Enrolled Students'])) if number(row['Enrolled Students']) is not None else None,
        'ReportedNonRegisteredStudents': int(number(row['Non Registered Students'])) if number(row['Non Registered Students']) is not None else None,
        'Capacity': int(number(row['Max Students(Group)'])) if number(row['Max Students(Group)']) is not None else None})

target = next(s for s in sections if s['SectionCode'] == '46252')
assert target['CourseCode'] == 'CSE213'
assert teachers[target['TeacherKey']]['FullName'].casefold() == 'christos stylianides'
courses['CSE213']['TeacherKey'] = target['TeacherKey']
assert len({s['SectionCode'] for s in sections}) == len(sections)
payload = {'Students': students, 'Teachers': list(teachers.values()), 'Courses': list(courses.values()),
           'Sections': sections, 'TargetCourseCode': 'CSE213', 'TargetSectionCode': '46252',
           'Term': 'F2026', 'EnrolledOn': '2026-10-02', 'ReplaceDemoSeed': args.deployment_bundle}
output = Path(args.output).resolve()
if args.deployment_bundle:
    if output != (Path(__file__).resolve().parent.parent / 'Demos/UniversityApi/SeedData/university-import.json'):
        raise SystemExit('Use Demos/UniversityApi/SeedData/university-import.json for the public deployment bundle.')
elif 'App_Data' not in output.parts:
    raise SystemExit('Use App_Data for a local import, or --deployment-bundle for the public Git snapshot.')
output.parent.mkdir(parents=True, exist_ok=True)
output.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({'students': len(students), 'teachers': len(teachers), 'courses': len(courses),
                  'sections': len(sections), 'targetSection': '46252',
                  'sectionsWithoutNamedInstructor': sum(s['TeacherKey'] is None for s in sections)}))
