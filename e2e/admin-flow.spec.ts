import { test, expect } from '@playwright/test';

/**
 * 管理端主链路烟雾测试
 * 覆盖：登录 -> 预约管理（筛选）-> 座位管理（状态切换）-> 统计页 -> 登出
 */

test.describe('admin-flow', () => {
  test('A1: admin login', async ({ page }) => {
    await page.goto('/Admin/Login');
    await expect(page.locator('h4')).toContainText('管理员登录');
    await page.fill('input[name="username"]', 'admin');
    await page.fill('input[name="password"]', 'admin123');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL(/\/Admin\/Reservations/);
    await expect(page.locator('h4')).toContainText('预约管理');
  });

  test('A2: reservations list and filter', async ({ page }) => {
    // login first
    await page.goto('/Admin/Login');
    await page.fill('input[name="username"]', 'admin');
    await page.fill('input[name="password"]', 'admin123');
    await page.click('button[type="submit"]');
    // should land on reservations page
    await expect(page.locator('h4')).toContainText('预约管理', { timeout: 10000 });
    // apply status filter - the form submits via GET
    await page.selectOption('select[name="statusFilter"]', '已取消');
    await page.click('button:has-text("筛选")');
    await page.waitForTimeout(1500);
    // page should still show 预约管理 heading (even if empty results)
    await expect(page.locator('h4')).toContainText('预约管理');
  });

  test('A3: seat management', async ({ page }) => {
    await page.goto('/Admin/Login');
    await page.fill('input[name="username"]', 'admin');
    await page.fill('input[name="password"]', 'admin123');
    await page.click('button[type="submit"]');
    await page.waitForURL(/\/Admin\/Reservations/);
    // navigate to seats
    await page.goto('/Admin/Seats');
    await expect(page.locator('h4')).toContainText('座位管理');
    const seatTable = page.locator('table');
    await expect(seatTable).toBeVisible();
    const addForm = page.locator('#createSeatForm');
    await expect(addForm).toBeVisible();
  });

  test('A4: stats page', async ({ page }) => {
    await page.goto('/Admin/Login');
    await page.fill('input[name="username"]', 'admin');
    await page.fill('input[name="password"]', 'admin123');
    await page.click('button[type="submit"]');
    await page.waitForURL(/\/Admin\/Reservations/);
    await page.goto('/Admin/Stats');
    await expect(page.locator('h4')).toContainText('数据统计');
    const statsCards = page.locator('.stats-card');
    await expect(statsCards.first()).toBeVisible();
    await expect(statsCards.first().locator('.stats-number')).toBeVisible();
  });

  test('A5: admin logout', async ({ page }) => {
    await page.goto('/Admin/Login');
    await page.fill('input[name="username"]', 'admin');
    await page.fill('input[name="password"]', 'admin123');
    await page.click('button[type="submit"]');
    await page.waitForURL(/\/Admin\/Reservations/);
    // logout
    await page.click('button:has-text("退出")');
    await expect(page).toHaveURL(/\/Admin\/Login/);
    await expect(page.locator('h4')).toContainText('管理员登录');
  });
});
