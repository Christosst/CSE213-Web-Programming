const { defineConfig, devices } = require('@playwright/test');
const fs = require('node:fs');
const path = require('node:path');
fs.mkdirSync('.test-data', { recursive: true });
module.exports = defineConfig({
  testDir: './tests', fullyParallel: false, workers: 1,
  forbidOnly: !!process.env.CI, retries: process.env.CI ? 1 : 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: { baseURL: 'http://localhost:3000', trace: 'retain-on-failure' },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } }
  ],
  webServer: [
    { command: `"${process.execPath}" tools/serve.cjs`, url: 'http://localhost:3000', reuseExistingServer: false },
    { command: 'dotnet run --project Demos/CourseApi --launch-profile classroom', url: 'http://localhost:5080/api/hello', reuseExistingServer: false, timeout: 60000 },
    { command: 'dotnet run --project Demos/SqliteNotesApi --launch-profile classroom', url: 'http://localhost:5081/api/notes', env: { ConnectionStrings__Notes: `Data Source=${path.resolve('.test-data/notes.db')}` }, reuseExistingServer: false, timeout: 60000 }
  ]
});
