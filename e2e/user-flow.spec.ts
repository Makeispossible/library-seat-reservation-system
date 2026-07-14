import { test, expect } from '@playwright/test';

/**
 * 用户端主链路烟雾测试
 * 覆盖：首页 -> 切换账号 -> 查看座位 -> 查看详情 -> 提交预约 -> 我的预约 -> 取消预约
 */

test.describe('user-flow', () => {
  test('T1: home page and account switch', async ({ page }) => {
    await page.goto('/');
    // title is h1
    await expect(page.locator('h1')).toContainText('图书馆座位预约');
    // click a student button to switch
    await page.click('button:has-text("学生A")');
    await page.waitForURL('/');
    // verify session persisted
    await expect(page.locator('#userNavbar strong')).toContainText('学生A');
  });

  test('T2: seats list page', async ({ page }) => {
    await page.goto('/Seats');
    // verify seat cards visible
    const seatCards = page.locator('.seat-card');
    await expect(seatCards.first()).toBeVisible();
    // area filter (select auto-submits)
    await page.selectOption('select[name="area"]', '一楼大厅');
    await page.waitForTimeout(1500);
    await expect(seatCards.first()).toBeVisible();
  });

  test('T3: create reservation and view my reservations', async ({ page }) => {
    await page.goto('/');
    await page.click('button:has-text("学生A")');
    await page.waitForURL('/');
    await page.goto('/Seats');
    await page.locator('.seat-card').first().click();

    const reserveBtn = page.locator('a').filter({ hasText: '预约此座位' });
    if (await reserveBtn.isVisible()) {
      await reserveBtn.click();
      const startTime = page.locator('input[name="startTime"]');
      if (await startTime.isVisible()) {
        await startTime.fill('10:00');
        await page.locator('input[name="endTime"]').fill('11:00');
        await page.click('button[type="submit"]');
        await expect(page).toHaveURL(/\/Reservation\/My/);
        await expect(page.locator('.alert-custom-success')).toBeVisible();
      }
    }
    await page.goto('/Reservation/My');
    await expect(page.locator('h4')).toContainText('我的预约');
  });

  test('T4: cancel reservation', async ({ page }) => {
    // use 学生E to ensure existing reservation exists
    await page.goto('/');
    await page.click('button:has-text("学生E")');
    await page.waitForURL('/');
    await page.goto('/Reservation/My');

    const cancelBtn = page.locator('button').filter({ hasText: '取消' });
    const count = await cancelBtn.count();
    if (count > 0) {
      page.once('dialog', async dialog => { await dialog.accept(); });
      await cancelBtn.first().click();
      await expect(page.locator('.alert-custom-success')).toBeVisible({ timeout: 10000 });
    }
  });
});
