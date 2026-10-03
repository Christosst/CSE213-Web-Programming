const { defineConfig, devices } = require('@playwright/test');
const fs = require('node:fs');
const path = require('node:path');
fs.mkdirSync('.test-data', { recursive: true });
const database = path.resolve('.test-data/university-shared-tests.db');
// Both test apps share this file. The real classroom database stays untouched.
if (process.env.TEST_WORKER_INDEX === undefined) {
  for (const suffix of ['', '-wal', '-shm']) fs.rmSync(database + suffix, { force: true });
}
module.exports = defineConfig({
  testDir: './tests', testMatch: 'universityweb.spec.js', workers: 1,
  reporter: 'list', timeout: 30000,
  use: { baseURL: 'http://localhost:5083', viewport: { width: 1120, height: 780 }, trace: 'retain-on-failure' },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'], viewport: { width: 1120, height: 780 } } }],
  webServer: [
    {
      command: 'dotnet run --project Demos/UniversityApi --launch-profile classroom --urls http://localhost:5092',
      url: 'http://localhost:5092/api/students', reuseExistingServer: false, timeout: 120000,
      env: { ConnectionStrings__University: `Data Source=${database};Foreign Keys=True`, 'SeedData__ImportPath': 'SeedData/university-import.json' }
    },
    {
      command: 'dotnet run --project Demos/UniversityWeb --launch-profile classroom',
      url: 'http://localhost:5083', reuseExistingServer: false, timeout: 60000,
      env: { ConnectionStrings__University: `Data Source=${database};Foreign Keys=True` }
    }
  ]
});
