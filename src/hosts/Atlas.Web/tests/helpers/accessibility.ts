import AxeBuilder from '@axe-core/playwright';
import type { Page } from '@playwright/test';

// Storybook's automatic a11y scan may briefly overlap this explicit scan.
// Retry only the known engine contention; never retry or suppress violations.
export async function analyzeAccessibility(page: Page, selector: string) {
  for (let attempt = 0; ; attempt++) {
    try {
      return await new AxeBuilder({ page }).include(selector).analyze();
    } catch (error) {
      if (attempt >= 10 || !String(error).includes('Axe is already running')) throw error;
      await page.waitForTimeout(200);
    }
  }
}
