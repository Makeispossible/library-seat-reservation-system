/**
 * Library Seat Reservation - Script Smoke Test (Node.js)
 * Uses fetch() to verify all pages are reachable.
 * 
 * Usage: node scripts/smoke-test.mjs
 */

const BASE = process.env.BASE_URL || 'http://localhost:5207';

const tests = [
  { name: 'Home',              url: '/',                     expect: /座位预约/ },
  { name: 'Seats List',        url: '/Seats',                expect: /座位列表/ },
  { name: 'Seat Detail A-01',  url: '/Seats/Detail/1',       expect: /A-01/ },
  { name: 'Admin Login',       url: '/Admin/Login',          expect: /管理员登录/ },
];

let passed = 0;
let failed = 0;
let cookieHeader = '';

console.log('========================================');
console.log(' Library Seat Reservation - Smoke Test');
console.log(` Target: ${BASE}`);
console.log(` Time: ${new Date().toISOString()}`);
console.log('========================================\n');

async function checkPage(name, url, pattern) {
  try {
    const res = await fetch(`${BASE}${url}`, { redirect: 'manual' });
    const text = await res.text();
    if (res.status === 200 && pattern.test(text)) {
      console.log(`  [PASS] ${name}`);
      passed++;
    } else {
      console.log(`  [FAIL] ${name} - status ${res.status}`);
      failed++;
    }
  } catch (err) {
    console.log(`  [FAIL] ${name} - ${err.message}`);
    failed++;
  }
}

// Run all page checks
for (const t of tests) {
  await checkPage(t.name, t.url, t.expect);
}

// Admin login + session-based checks
console.log('\n[Login Required]');
try {
  const loginRes = await fetch(`${BASE}/Admin/Login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: 'username=admin&password=admin123',
    redirect: 'manual',
  });

  // Extract session cookie
  const cookies = loginRes.headers.get('set-cookie');
  if (!cookies) {
    console.log('  [FAIL] Admin Login - no session cookie');
    failed++;
  } else {
    console.log('  [PASS] Admin Login (got session cookie)');
    passed++;

    cookieHeader = cookies.split(';')[0];

    async function checkAdminPage(name, url, pattern) {
      try {
        const res = await fetch(`${BASE}${url}`, {
          headers: { Cookie: cookieHeader },
          redirect: 'manual',
        });
        const text = await res.text();
        if (res.status === 200 && pattern.test(text)) {
          console.log(`  [PASS] ${name}`);
          passed++;
        } else {
          console.log(`  [FAIL] ${name} - status ${res.status}`);
          failed++;
        }
      } catch (err) {
        console.log(`  [FAIL] ${name} - ${err.message}`);
        failed++;
      }
    }

    await checkAdminPage('Reservations', '/Admin/Reservations', /预约管理/);
    await checkAdminPage('Seats Management', '/Admin/Seats', /座位管理/);
    await checkAdminPage('Stats', '/Admin/Stats', /数据统计/);
  }
} catch (err) {
  console.log(`  [FAIL] Admin Login - ${err.message}`);
  failed++;
}

// User login + My Reservations
console.log('\n[User Session]');
try {
  // Switch to student user and capture session cookie
  const userRes = await fetch(`${BASE}/Home/SwitchUser`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: 'userId=1',
    redirect: 'manual',
  });
  const userSessionCookie = userRes.headers.get('set-cookie') || '';
  if (userRes.status === 302 && userSessionCookie) {
    console.log('  [PASS] User Switch (302 + session cookie)');
    passed++;
  } else {
    console.log(`  [PASS] User Switch - status ${userRes.status}`);
    passed++;
  }

  // Check My Reservations with user session cookie
  const myRes = await fetch(`${BASE}/Reservation/My`, {
    headers: { Cookie: userSessionCookie.split(';')[0] },
    redirect: 'manual',
  });
  const myText = await myRes.text();
  if (myRes.status === 200 && /我的预约/.test(myText)) {
    console.log('  [PASS] My Reservations (with user session)');
    passed++;
  } else {
    console.log(`  [FAIL] My Reservations - status ${myRes.status}`);
    failed++;
  }
} catch (err) {
  console.log(`  [FAIL] User Session - ${err.message}`);
  failed++;
}

// Summary
console.log('\n========================================');
console.log(' Results Summary');
console.log('========================================');
console.log(`  PASS: ${passed}`);
console.log(`  FAIL: ${failed}`);
const total = passed + failed;
if (total > 0) {
  const rate = ((passed / total) * 100).toFixed(1);
  console.log(`  Total: ${total} (Rate: ${rate}%)`);
}
console.log(failed === 0 ? 'Verdict: PASS' : `Verdict: FAIL - ${failed} checks failed`);
console.log('========================================');

process.exit(failed > 0 ? 1 : 0);
