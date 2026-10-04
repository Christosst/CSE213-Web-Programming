const { test, expect } = require('@playwright/test');
const fs = require('node:fs/promises');
const path = require('node:path');

const pictures = path.join(__dirname, '..', '.test-data', 'universityweb-pictures');
const api = 'http://localhost:5092/api';
test.describe.configure({ mode: 'serial' });

async function submit(page, url, values, button) {
  await page.goto(url);
  for (const [name, value] of Object.entries(values)) {
    const input = page.locator(`[name="Input.${name}"]`);
    if (await input.evaluate(el => el.tagName === 'SELECT'))
      await input.selectOption(value);
    else await input.fill(value);
  }
  await page.getByRole('button', { name: button, exact: true }).click();
}

async function rowId(page, text) {
  const row = page.getByRole('row').filter({ hasText: text });
  const href = await row.getByRole('link', { name: 'Details', exact: true }).getAttribute('href');
  return href.split('/').at(-1);
}

test('seeded class, HTML response, accessible navigation and screenshots', async ({ page }) => {
  await fs.mkdir(pictures, { recursive: true });
  const response = await page.goto('/');
  expect(response.headers()['content-type']).toContain('text/html');
  await expect(page.getByRole('heading', { name: 'University classroom' })).toBeVisible();
  await expect(page.locator('.stats')).toContainText('20');
  await expect(page.locator('.stats')).toContainText('Teachers: 719');
  await expect(page.locator('.stats')).toContainText('Courses: 1002');
  await page.screenshot({ path: path.join(pictures, 'home.png') });
  await page.getByRole('link', { name: 'Students', exact: true }).click();
  expect(await page.locator('tbody tr').count()).toBe(20);
  await page.screenshot({ path: path.join(pictures, 'students.png') });
  await page.goto('/Teachers');
  await expect(page.getByRole('cell', { name: 'Christos Stylianides', exact: true })).toBeVisible();
  await page.goto('/Enrollments');
  expect(await page.locator('tbody tr').count()).toBe(20);
  await expect(page.locator('tbody')).toContainText('CSE213');
  await page.screenshot({ path: path.join(pictures, 'enrollments.png') });
});

test('server validation keeps values, catches duplicates and rejects missing CSRF tokens', async ({ page, request }) => {
  await page.goto('/Students/Create');
  await page.locator('form').evaluate(form => form.noValidate = true);
  await page.getByLabel('Full name', { exact: true }).fill('Classroom Example');
  await page.getByRole('button', { name: 'Create student', exact: true }).click();
  await expect(page.locator('.validation-summary-errors')).toContainText('Registration number');
  await expect(page.getByLabel('Full name', { exact: true })).toHaveValue('Classroom Example');
  await page.screenshot({ path: path.join(pictures, 'validation.png') });
  await page.goto('/Students');
  const registration = await page.locator('tbody tr').first().locator('td').nth(1).innerText();
  await submit(page, '/Students/Create', { FullName: 'Duplicate Example', RegistrationNumber: registration }, 'Create student');
  await expect(page.locator('.validation-summary-errors')).toContainText('already exists');
  const forged = await request.post('/Students/Create', { form: { 'Input.FullName': 'Forged', 'Input.RegistrationNumber': 'FORGED' } });
  expect(forged.status()).toBe(400);
});

test('complete four-entity CRUD, relationship guards, escaping and route protection', async ({ page, request }) => {
  await submit(page, '/Teachers/Create', { FullName: 'Demo Lecturer', Email: 'lecturer@example.test' }, 'Create teacher');
  await expect(page.getByRole('status')).toHaveText('Teacher created.');
  const teacherId = await rowId(page, 'Demo Lecturer');
  await submit(page, `/Teachers/Edit/${teacherId}`, { FullName: 'Demo Lecturer Updated' }, 'Save changes');
  await expect(page.locator('tbody')).toContainText('Demo Lecturer Updated');
  await submit(page, '/Courses/Create', { Code: 'DEMO301', Title: 'Razor Practice', TeacherId: teacherId }, 'Create course');
  const courseId = await rowId(page, 'DEMO301');
  await submit(page, `/Courses/Edit/${courseId}`, { Title: 'Razor Practice Updated' }, 'Save changes');
  await expect(page.locator('tbody')).toContainText('Razor Practice Updated');

  const name = '<img src=x onerror="window.demoUnsafe=true">';
  await submit(page, '/Students/Create', { FullName: name, RegistrationNumber: 'RAZOR-TEST', Email: 'student@example.test' }, 'Create student');
  const studentId = await rowId(page, 'RAZOR-TEST');
  expect(await page.evaluate(() => window.demoUnsafe)).toBeUndefined();
  expect(await page.locator('tbody img').count()).toBe(0);
  await submit(page, `/Students/Edit/${studentId}`, { FullName: 'Demo Student' }, 'Save changes');
  await page.goto(`/Students/Details/${studentId}`);
  await expect(page.locator('.details')).toContainText('Demo Student');
  await page.goto(`/Students/Delete/${studentId}`);
  await page.getByRole('link', { name: 'Cancel', exact: true }).click();
  await expect(page.locator('tbody')).toContainText('Demo Student');

  await submit(page, '/Enrollments/Create', { StudentId: studentId, CourseId: courseId, EnrolledOn: '2026-10-05' }, 'Create enrollment');
  const enrollmentId = await rowId(page, 'Demo Student');
  await submit(page, `/Enrollments/Edit/${enrollmentId}`, { EnrolledOn: '2026-10-12' }, 'Save changes');
  await expect(page.locator('tbody')).toContainText('2026-10-12');
  await submit(page, '/Enrollments/Create', { StudentId: studentId, CourseId: courseId, EnrolledOn: '2026-10-05' }, 'Create enrollment');
  await expect(page.locator('.validation-summary-errors')).toContainText('already enrolled');

  for (const [entity, id] of [['Teachers', teacherId], ['Courses', courseId], ['Students', studentId]]) {
    await page.goto(`/${entity}/Delete/${id}`);
    await page.getByRole('button', { name: 'Confirm delete', exact: true }).click();
    await expect(page.locator('.validation-summary-errors')).toContainText('still uses');
  }
  for (const [entity, id] of [['Enrollments', enrollmentId], ['Students', studentId], ['Courses', courseId], ['Teachers', teacherId]]) {
    await page.goto(`/${entity}/Delete/${id}`);
    await page.getByRole('button', { name: 'Confirm delete', exact: true }).click();
    await expect(page.getByRole('status')).toContainText('deleted');
    expect((await request.get(`/${entity}/Details/${id}`)).status()).toBe(404);
  }
  expect((await request.get('/Students/Edit/not-an-integer')).status()).toBe(404);
  expect((await request.get('/Students/Details/2147483647')).status()).toBe(404);
});

test('tampered foreign keys cannot create records and dropdowns survive validation', async ({ page }) => {
  await page.goto('/Courses/Create');
  await page.locator('select').evaluate(select => select.add(new Option('Tampered', '2147483647')));
  await page.getByLabel('Course code').fill('BADKEY');
  await page.getByLabel('Title', { exact: true }).fill('Invalid teacher');
  await page.getByLabel('Teacher', { exact: true }).selectOption('2147483647');
  await page.getByRole('button', { name: 'Create course', exact: true }).click();
  await expect(page.locator('.validation-summary-errors')).toContainText('existing teacher');
  await expect(page.getByLabel('Teacher', { exact: true })).toContainText('Christos Stylianides');
  await page.goto('/Courses');
  await expect(page.locator('tbody')).not.toContainText('BADKEY');
  await page.goto('/Students/Create');
  await page.getByLabel('Full name', { exact: true }).fill('Classroom Example');
  await page.getByLabel('Registration number').fill('RAZOR-DEMO');
  await page.screenshot({ path: path.join(pictures, 'create.png') });
});

test('data remains available for a separate restart check', async ({ page }) => {
  await submit(page, '/Teachers/Create', { FullName: 'Persistence Check', Email: 'restart@example.test' }, 'Create teacher');
  await expect(page.locator('tbody')).toContainText('Persistence Check');
});

test('API and Razor Pages see the same writes and preserve university metadata', async ({ page, request }) => {
  const created = await request.post(`${api}/students`, { data: {
    fullName: 'API Shared Student', registrationNumber: 'SHARED-TEST',
    program: 'Computer Science', department: 'Computer Science & Engineering', cumulativeGpa: 3.5
  } });
  expect(created.status()).toBe(201);
  const student = await created.json();
  await page.goto(`/Students/Details/${student.id}`);
  await expect(page.locator('.details')).toContainText('API Shared Student');
  await submit(page, `/Students/Edit/${student.id}`, { FullName: 'Razor Shared Student' }, 'Save changes');
  let saved = await (await request.get(`${api}/students/${student.id}`)).json();
  expect(saved.fullName).toBe('Razor Shared Student');
  expect(saved.program).toBe('Computer Science');
  expect(saved.department).toBe('Computer Science & Engineering');
  expect(saved.cumulativeGpa).toBe(3.5);
  expect((await request.put(`${api}/students/${student.id}`, { data: { ...saved, fullName: 'API Updated Student' } })).status()).toBe(204);
  await page.goto(`/Students/Details/${student.id}`);
  await expect(page.locator('.details')).toContainText('API Updated Student');
  await page.goto(`/Students/Delete/${student.id}`);
  await page.getByRole('button', { name: 'Confirm delete', exact: true }).click();
  expect((await request.get(`${api}/students/${student.id}`)).status()).toBe(404);

  const courseResponse = await request.post(`${api}/courses`, { data: { code: 'SHARED401', title: 'Course without a teacher', teacherId: null, ects: 6 } });
  expect(courseResponse.status()).toBe(201);
  const course = await courseResponse.json();
  await page.goto('/Courses');
  await expect(page.getByRole('row').filter({ hasText: 'SHARED401' })).toContainText('Not assigned');
  await submit(page, `/Courses/Edit/${course.id}`, { Title: 'Edited in Razor' }, 'Save changes');
  const savedCourse = await (await request.get(`${api}/courses/${course.id}`)).json();
  expect(savedCourse.title).toBe('Edited in Razor');
  expect(savedCourse.teacherId).toBeNull();
  expect(savedCourse.ects).toBe(6);
  expect((await request.delete(`${api}/courses/${course.id}`)).status()).toBe(204);

  const seeded = (await (await request.get(`${api}/enrollments`)).json())[0];
  await submit(page, `/Enrollments/Edit/${seeded.id}`, { EnrolledOn: seeded.enrolledOn }, 'Save changes');
  const savedEnrollment = await (await request.get(`${api}/enrollments/${seeded.id}`)).json();
  expect(savedEnrollment.courseSectionId).toBe(seeded.courseSectionId);
  expect(savedEnrollment.courseSectionId).not.toBeNull();
});

test('simple pages fit a narrow viewport and have no client framework scripts', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  for (const url of ['/', '/Students', '/Students/Create', '/Courses/Create', '/HowItWorks']) {
    await page.goto(url);
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(390);
    expect(await page.locator('script').count()).toBe(0);
  }
});
