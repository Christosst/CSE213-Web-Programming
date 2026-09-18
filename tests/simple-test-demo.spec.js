// @ts-check
import { test, expect } from '@playwright/test';

test('First Page Demo', async ({ page }) => {
  await page.goto('http://cse213.runasp.net/01%20-%20Introduction/Ch01%20-%20Get%20Started/demo-first-page.html');

  // Expect a title "to contain" a substring.
  await expect(page).toHaveTitle(/CSE213/);
});

test('Progressive Enhancement Demo', async ({ page }) => {
  await page.goto('http://cse213.runasp.net/01%20-%20Introduction/Ch03%20-%20Big%20Concepts/demo-progressive-enhancement.html');

  // Click the get started link.
  await page.getByRole('button', { name: 'Click me' }).click();

  // Expects page to have a heading with the name of Installation.
  await expect(page.locator('#message')).toHaveText('JavaScript is active — behaviour layer working!');
});
