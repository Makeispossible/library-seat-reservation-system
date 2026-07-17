import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  timeout: 60000,
  retries: 0,
  use: {
    baseURL: 'http://localhost:5207',
    channel: 'msedge',
    headless: true,
    screenshot: 'only-on-failure',
    trace: 'retain-on-failure',
  },
  webServer: {
    command: 'dotnet run --project src/LibrarySeatReservation.Web/LibrarySeatReservation.Web.csproj --urls http://localhost:5207',
    port: 5207,
    reuseExistingServer: true,
    timeout: 60000,
  },
});
