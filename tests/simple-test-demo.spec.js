const { test, expect } = require('@playwright/test');
const chapter = {
  first: '/01 - Introduction/Ch01 - Get Started/demo-first-page.html',
  enhancement: '/01 - Introduction/Ch03 - Big Concepts/demo-progressive-enhancement.html',
  form: '/02 - HTML/Ch09 - Forms/demo-accessible-form.html',
  average: '/04 - JavaScript/Ch22 - Intro to JavaScript/demo-js-fundamentals.html',
  products: '/04 - JavaScript/Ch23 - Functions and Loops/demo-arrays-modules/index.html',
  tasks: '/04 - JavaScript/Ch24 - DOM and Events/demo-dom-events.html',
  fetch: '/04 - JavaScript/Ch25 - Next Level JS/demo-fetch-states.html',
  storage: '/04 - JavaScript/Ch25 - Next Level JS/demo-local-storage.html'
};

test('catalogue links and core demo assets return 200', async ({ page, request }) => {
  await page.goto('/');
  const urls = await page.locator('main a').evaluateAll(links => links.map(link => link.href));
  for (const url of urls) {
    if (url.startsWith('http://localhost:3000')) expect((await request.get(url)).status(), url).toBe(200);
  }
  const core = await page.locator('.demo a').evaluateAll(links => links.map(link => link.href));
  for (const url of core) {
    const failures = [];
    const handler = response => { if (response.url().startsWith('http://localhost:3000') && response.status() >= 400) failures.push(response.url()); };
    page.on('response', handler);
    await page.goto(url);
    await page.waitForLoadState('networkidle');
    expect(failures, url).toEqual([]);
    await expect(page.locator('h1')).toHaveCount(1);
    page.off('response', handler);
  }
});

test('first page has page landmarks and standards mode', async ({ page }) => {
  await page.goto(chapter.first);
  for (const role of ['banner','main','contentinfo']) await expect(page.getByRole(role)).toHaveCount(1);
  expect(await page.evaluate(() => document.compatMode)).toBe('CSS1Compat');
  await page.goto(chapter.first.replace('demo-first-page.html', 'demo-first-page-nc.html'));
  expect(await page.evaluate(() => document.compatMode)).toBe('BackCompat');
});

test('progressive enhancement has readable baseline and keyboard greeting', async ({ page, browser }) => {
  const context = await browser.newContext({ javaScriptEnabled: false });
  const baseline = await context.newPage();
  await baseline.goto('http://localhost:3000' + chapter.enhancement);
  await expect(baseline.getByRole('heading', { name: 'What to bring' })).toBeVisible();
  await context.close();
  await page.goto(chapter.enhancement);
  await page.getByRole('button', { name: 'Click me' }).focus();
  await page.keyboard.press('Enter');
  await expect(page.getByRole('status')).toHaveText('Hello! JavaScript is working.');
});

test('native form validates and safely echoes named fields', async ({ page }) => {
  await page.goto(chapter.form);
  await page.getByRole('button', { name: 'Send form' }).click();
  expect(await page.locator('form').evaluate(form => form.checkValidity())).toBe(false);
  await page.getByLabel('Name', { exact: true }).fill('<img src=x>');
  await page.getByLabel('Email').fill('alex@example.org');
  await page.getByLabel('Topic').selectOption('CSS');
  await page.getByRole('button', { name: 'Send form' }).click();
  await expect(page.getByRole('heading', { name: 'Form received' })).toBeVisible();
  await expect(page.locator('pre')).toContainText('<img src=x>');
  expect(JSON.parse(await page.locator('pre').textContent()).topic).toBe('CSS');
  await expect(page.locator('img')).toHaveCount(0);
});

test('average uses numeric arithmetic and rejects blank input', async ({ page }) => {
  await page.goto(chapter.average);
  await page.getByRole('button', { name: 'Calculate' }).click();
  await expect(page.getByRole('status')).toHaveText('Practice average: 70.0');
  await page.getByLabel('First score (0–100)').fill('');
  await page.getByRole('button', { name: 'Calculate' }).click();
  expect(await page.locator('form').evaluate(form => form.checkValidity())).toBe(false);
  await page.getByLabel('First score (0–100)').fill('10');
  await page.getByLabel('Second score (0–100)').fill('20');
  await page.getByRole('button', { name: 'Calculate' }).click();
  await expect(page.getByRole('status')).toHaveText('Practice average: 15.0');
});

test('module filter and loop/reduce return known results', async ({ page }) => {
  await page.goto(chapter.products);
  await expect(page.getByRole('status')).toHaveText('Total: €18.00');
  await page.getByLabel('Category').selectOption('stationery');
  await expect(page.locator('#products li')).toHaveCount(2);
  await expect(page.getByRole('status')).toHaveText('Total: €6.00');
  const totals = await page.evaluate(async () => {
    const { products } = await import('./data.js');
    const { totalPrice, totalWithLoop } = await import('./utils.js');
    return [totalPrice(products), totalWithLoop(products), totalPrice([])];
  });
  expect(totals).toEqual([18, 18, 0]);
});

test('task delegation supports new controls and treats titles as text', async ({ page }) => {
  await page.goto(chapter.tasks);
  await page.getByLabel('New task').fill('<img src=x onerror=alert(1)>');
  await page.getByRole('button', { name: 'Add task' }).click();
  await expect(page.locator('#tasks li')).toHaveCount(1);
  await expect(page.locator('#tasks img')).toHaveCount(0);
  await page.getByRole('checkbox').check();
  await expect(page.locator('#tasks li')).toHaveClass('completed');
  await page.getByRole('button', { name: 'Delete' }).click();
  await expect(page.getByRole('status')).toHaveText('No tasks yet.');
});

test('fetch shows loading, success, empty, HTTP and network failure', async ({ page }) => {
  await page.goto(chapter.fetch);
  await page.route('**/data/users.json', async route => {
    await new Promise(resolve => setTimeout(resolve, 250));
    await route.continue();
  });
  await page.getByRole('button', { name: 'Load users' }).click();
  await expect(page.getByRole('status')).toHaveText('Loading...');
  await expect(page.getByRole('button', { name: 'Load users' })).toBeDisabled();
  await expect(page.getByRole('status')).toHaveText('Users loaded.');
  await expect(page.locator('#users li')).toHaveCount(2);
  await page.getByRole('button', { name: 'Empty result' }).click();
  await expect(page.getByRole('status')).toHaveText('No users found.');
  await page.getByRole('button', { name: 'HTTP error' }).click();
  await expect(page.getByRole('status')).toContainText('HTTP 404');
  await page.route('**/data/users.json', route => route.abort());
  await page.getByRole('button', { name: 'Load users' }).click();
  await expect(page.getByRole('status')).toContainText('Could not load users:');
  await expect(page.getByRole('button', { name: 'Load users' })).toBeEnabled();
});

test('storage survives reload and forget affects only its own key', async ({ page }) => {
  await page.goto(chapter.storage);
  await page.evaluate(() => localStorage.setItem('unrelated', 'keep'));
  await page.getByLabel('Practice username (no sensitive data)').fill('Alex');
  await page.getByRole('button', { name: 'Remember', exact: true }).click();
  await page.reload();
  await expect(page.locator('#username')).toHaveValue('Alex');
  await page.getByRole('button', { name: 'Forget' }).click();
  expect(await page.evaluate(() => localStorage.getItem('unrelated'))).toBe('keep');
  await page.reload();
  await expect(page.locator('#username')).toHaveValue('');
});

test('REST tasks implement Location, 204, 404, filtering and validation', async ({ request }) => {
  const base = 'http://localhost:5080';
  const created = await request.post(base + '/api/tasks', { data: { title: '  API test  ', isCompleted: false } });
  expect(created.status()).toBe(201);
  const task = await created.json();
  expect(task.title).toBe('API test');
  const location = created.headers().location;
  expect(location).toContain(`/api/tasks/${task.id}`);
  expect((await request.get(location)).status()).toBe(200);
  expect((await request.put(location, { data: { title: task.title, isCompleted: true } })).status()).toBe(204);
  expect(await (await request.get(base + '/api/tasks?completed=false')).json()).not.toContainEqual(expect.objectContaining({ id: task.id }));
  const deleted = await request.delete(location);
  expect(deleted.status()).toBe(204);
  expect(await deleted.body()).toHaveLength(0);
  expect((await request.get(location)).status()).toBe(404);
  expect((await request.put(location, { data: { title: 'Missing' } })).status()).toBe(404);
  for (const data of [{}, { title: '  ' }, { title: 'x'.repeat(121) }]) expect((await request.post(base + '/api/tasks', { data })).status()).toBe(400);
});

test('text contracts, validation and Swagger documents are correct', async ({ request }) => {
  const base = 'http://localhost:5080';
  expect(await (await request.post(base + '/api/text/analysis', { data: { text: 'Hello web' } })).json()).toEqual({ characters: 9, words: 2 });
  expect(await (await request.post(base + '/api/text/analysis', { data: { text: 'a\t b\n' } })).json()).toEqual({ characters: 5, words: 2 });
  expect(await (await request.post(base + '/api/text/uppercase', { data: { text: 'Hello web' } })).json()).toEqual({ text: 'HELLO WEB' });
  for (const data of [{}, { text: null }, { text: '  ' }, { text: 'x'.repeat(5001) }]) expect((await request.post(base + '/api/text/analysis', { data })).status()).toBe(400);
  expect((await request.post(base + '/api/text/analysis', { data: '{bad json', headers: { 'Content-Type': 'application/json' } })).status()).toBe(400);
  const core = await (await request.get(base + '/openapi/v1.json')).json();
  expect(core.paths['/api/tasks'].post.responses['201']).toBeTruthy();
  expect(core.paths['/api/text/analysis']).toBeTruthy();
});

test('API frontend submits text and safely creates, updates and deletes tasks', async ({ page }) => {
  await page.goto('http://localhost:5080/text.html');
  await page.getByRole('button', { name: 'Analyze', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('Done.');
  await expect(page.locator('#result')).toContainText('"words": 2');
  await page.getByLabel('Text (1–5000 characters)').fill('  ');
  await page.getByRole('button', { name: 'Analyze', exact: true }).click();
  await expect(page.getByRole('status')).toContainText('required');
  await page.goto('http://localhost:5080/tasks.html');
  await expect(page.getByRole('status')).toHaveText('No tasks yet.');
  await page.getByLabel('Task title').fill('<img src=x>');
  await page.getByRole('button', { name: 'Add task' }).click();
  await expect(page.getByRole('status')).toHaveText('Tasks loaded.');
  await expect(page.locator('#tasks img')).toHaveCount(0);
  await page.getByRole('button', { name: 'Complete', exact: true }).click();
  await expect(page.locator('#tasks')).toContainText('(complete)');
  await page.getByRole('button', { name: 'Delete', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('No tasks yet.');
});


test('registration lab validates patterns and submits all control types', async ({ page }) => {
  await page.goto('/02 - HTML/Ch09 - Forms/studentlab.html');
  await page.getByLabel('Full name').fill('Practice Student');
  await page.getByLabel('Email', { exact: true }).fill('student@example.org');
  await page.getByLabel('Topic', { exact: true }).selectOption('html');
  await page.getByLabel('Practice passport code (optional)').fill('bad');
  expect(await page.locator('form').evaluate(form => form.checkValidity())).toBe(false);
  await page.getByLabel('Practice passport code (optional)').fill('AB1234CD5');
  await page.getByLabel('Study year').fill('2');
  await page.getByLabel('Preferred session date (optional)').fill('2026-10-15');
  await page.getByLabel('Notes (optional)').fill('Practice payload');
  await page.getByRole('button', { name: 'Submit practice form' }).click();
  const data = JSON.parse(await page.locator('pre').textContent());
  expect(data).toMatchObject({ topic: 'html', code: 'AB1234CD5', year: '2', date: '2026-10-15', notes: 'Practice payload' });
});

test('box sizes match calculations and brief modal handles keyboard focus', async ({ page }) => {
  await page.goto('/03 - CSS/Ch15 - Box Model/demo-box-model.html');
  expect(await page.locator('.content').evaluate(node => node.getBoundingClientRect().width)).toBe(372);
  expect(await page.locator('.border').evaluate(node => node.getBoundingClientRect().width)).toBe(320);
  await page.goto('/03 - CSS/Ch16 - Floating and Positioning/demo-dialog.html');
  const trigger = page.getByRole('button', { name: 'Open dialog' });
  await trigger.click();
  await expect(page.getByRole('dialog')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Close', exact: true })).toBeFocused();
  await page.keyboard.press('Escape');
  await expect(page.getByRole('dialog')).not.toBeVisible();
  await expect(trigger).toBeFocused();
});

test('catalogue fits desktop and mobile widths', async ({ page }) => {
  const output = process.env.CSE213_REVIEW_SCREENSHOTS;
  for (const width of [1280, 390]) {
    await page.setViewportSize({ width, height: 900 });
    await page.goto('/');
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(width);
    if (output) await page.screenshot({ path: `${output}/catalogue-${width}.png`, fullPage: true });
  }
});
