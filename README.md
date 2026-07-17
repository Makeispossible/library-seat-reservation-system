# 图书馆座位预约系统

图书馆座位预约系统是一个轻量级 Web 应用，让学生可以通过浏览器在线查看座位空闲情况、预约座位、取消预约；管理员可以在后台管理座位、查看预约记录，并可不受时间限制强制取消任何预约。系统采用体验账号切换方式，无需注册即可使用。

---

## 技术栈

| 层 | 技术 | 版本 |
|----|------|------|
| 框架 | ASP.NET Core MVC | .NET 9 |
| ORM | Entity Framework Core | 9.x |
| 数据库 | SQL Server LocalDB | 内置 |
| 前端 | Bootstrap | 5.3 (CDN) |
| 前端扩展 | `css/style.css` | 自定义样式 |
| 自动化测试 | Playwright (msedge) | `@playwright/test` |
| 脚本测试 | Node.js | `scripts/smoke-test.mjs` |

> 降级方案：如果 LocalDB 不可用，可切换 SQLite（修改 Program.cs 中 `UseSqlServer` → `UseSqlite`，连接字符串改为 `Data Source=LibrarySeat.db`）。

---

## 目录结构

```
LibrarySeatReservation/         ← 仓库根目录
├── database/                   ← 数据库初始化说明
│   └── README.md
├── docs/                       ← 项目文档（按阶段编号）
│   ├── 01-项目立项单.md & -审计.md
│   ├── 02-需求分析与MVP确认.md & -审计.md
│   ├── 03-PRD-Lite.md & -审计.md
│   ├── 04-页面树与业务流程.md & -审计.md
│   ├── 05-页面卡与UI规范.md & -审计.md
│   ├── 06-静态原型与原型评审.md & -审计.md
│   ├── 07-系统设计说明.md & -审计.md
│   ├── 08-数据库设计.md & -审计.md
│   ├── 09-关键链路详细设计.md & -审计.md
│   ├── 10-开发准备与Sprint0.md & -审计.md
│   ├── 11-开发前一致性总审计.md & -审计.md
│   ├── 12-开发起步与骨架记录.md & -审计.md
│   ├── 13-用户端主链路开发记录.md & -审计.md
│   ├── 14-管理端与权限开发记录.md & -审计.md
│   ├── 15-功能完善与体验优化记录.md & -审计.md
│   ├── 16-联调测试与缺陷闭环.md & -审计.md
│   ├── 17-交付说明与项目复盘.md & -审计.md
│   └── 项目任务板与迭代记录.md
├── prototype/                  ← 交互原型
│   ├── static-v1/              ← 9 个 HTML 页面
│   │   ├── index.html
│   │   ├── seats.html, seat-detail.html, reservation-create.html, my-reservations.html
│   │   ├── admin-login.html, admin-reservations.html, admin-seats.html, admin-stats.html
│   │   └── css/style.css
│   └── review-1/               ← 原型评审清单
│       └── 原型评审清单.md
├── src/
│   └── LibrarySeatReservation.Web/  ← ASP.NET Core MVC 项目
│       ├── Controllers/
│       │   ├── HomeController.cs
│       │   ├── SeatsController.cs
│       │   ├── ReservationController.cs
│       │   └── AdminController.cs
│       ├── Models/
│       │   ├── StudentUser.cs, Seat.cs, Reservation.cs
│       │   ├── SeatDisplay.cs, ErrorViewModel.cs
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── DbInitializer.cs
│       ├── Helpers/StatusHelper.cs
│       ├── Filters/AdminAuthFilter.cs
│       ├── Views/
│       │   ├── Home/ + Seats/ + Reservation/ + Admin/ + Shared/
│       │   ├── _ViewImports.cshtml + _ViewStart.cshtml
│       ├── wwwroot/css/style.css
│       ├── Program.cs
│       ├── appsettings.json
│       └── LibrarySeatReservation.Web.csproj
├── e2e/                       ← Playwright E2E 测试
│   ├── user-flow.spec.ts
│   └── admin-flow.spec.ts
├── scripts/
│   └── smoke-test.mjs         ← Node.js 脚本烟雾测试
├── playwright.config.ts       ← Playwright 配置（msedge + baseURL）
├── package.json
├── LibrarySeatReservation.sln
├── README.md
└── opencode.json
```

---

## 运行前提

| 依赖 | 说明 |
|------|------|
| .NET 9 SDK | `dotnet --version` 确认 ≥ 9.0 |
| SQL Server LocalDB | `SqlLocalDB info` 确认可用 |
| 浏览器 | 现代浏览器（Chrome / Edge / Firefox） |
| Node.js | ≥ 18（运行烟雾测试 `node scripts/smoke-test.mjs`） |
| npm | 可选（安装 Playwright E2E 测试时需用） |

---

## 快速启动

```bash
# 1. 进入项目目录
cd LibrarySeatReservation

# 2. 还原 NuGet 包 + 建库（首次需要手动执行迁移）
dotnet restore
dotnet ef database update --project src/LibrarySeatReservation.Web

# 3. 运行
dotnet run --project src/LibrarySeatReservation.Web
```

浏览器访问 `http://localhost:5207` 即可看到首页。首次启动时种子数据自动写入。

---

## 自动化验证

### 安装 E2E 依赖（仅首次）

```powershell
# PowerShell 需先绕过执行策略
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force
npm install
```

### 运行 Playwright 自动点击测试

```powershell
npx playwright test --reporter=list
```
使用系统 Microsoft Edge 驱动，覆盖用户端（4 用例）+ 管理端（5 用例）主链路。

### 运行脚本烟雾测试

```powershell
node scripts/smoke-test.mjs
```
使用 Node.js 原生 fetch() 检查 10 个页面可达性，不依赖第三方包。**这是最快速的健康检查方式。**

### 运行 Playwright 有头模式

```powershell
npx playwright test --reporter=list --headed
```

---

## 已实现范围

| 模块 | 状态 | 说明 |
|------|------|------|
| 项目骨架 | ✅ 已完成 | .sln + .csproj + NuGet + Program.cs + appsettings |
| 数据库 | ✅ 已完成 | EF Core Code First 建 3 表 + 4 索引 + 种子数据 |
| 体验账号切换 | ✅ 已验证通过 | Session 持久化正常，5 学生可切换 |
| 座位列表 + 筛选 | ✅ 已验证通过 | 12 座位网格 + 区域/楼层筛选正常 |
| 座位详情 + 时段 | ✅ 已验证通过 | 详情渲染 + 预约按钮守卫正常 |
| 预约提交 + 冲突校验 | ✅ 已完成 | 表单校验 + 冲突检测正常 |
| 我的预约 + 取消 | ✅ 已验证通过 | 预约查看 + 取消按钮 + 日期感知守卫正常 + ViewBag 类型转换修复 |
| 管理员登录 | ✅ 已验证通过 | Session 持久化 + [AdminAuth] 守卫正常 |
| 座位管理 | ✅ 已验证通过 | 添加座位 + 状态切换 Available ↔ Maintenance |
| 预约记录查看 | ✅ 已验证通过 | 按日期/状态/用户筛选 + 管理员不受时间限制强制取消 |
| 统计页 | ✅ 已验证通过 | 实时 DB 计算：总数/今日预约/使用率/区域分布 |
| E2E 自动测试 | ✅ 9/9 通过 | Playwright (msedge) + Node.js 脚本烟雾测试 |

---

## 数据库初始化方式

### 首次部署

```bash
dotnet ef database update --project src/LibrarySeatReservation.Web
```

### 每次启动

`Program.cs` 中 `db.Database.Migrate()` 自动执行挂起的迁移，随后 `DbInitializer.Initialize()` 写入种子数据（检测到表非空则跳过）。

---

## 演示账号

| 角色 | 账号 | 说明 |
|------|------|------|
| 学生体验账号 | 学生A ~ 学生E | 首页下拉框切换，无需密码 |
| 管理员 | admin / admin123 | 访问 `/Admin/Login` 登录 |

---

## 已知限制

> 此段落将持续更新已发现但未修复的已知问题。

| 编号 | 问题 | 严重度 | 备注 |
|------|------|--------|------|
| TD-01 | `favicon.ico` 缺失（浏览器控制台 404） | P4 | 不影响功能 |
| TD-04 | `LibrarySeatReservation.Web.styles.css` 404 | P4 | ASP.NET Core 可选文件，可忽略 |
| TD-05 | Stats 视图未渲染 AvailableSeats / MaintenanceSeats | P3 | 下轮修复 |
| TD-06 | 导航栏当前页未高亮（用户端 + 管理端） | P3 | 下一轮 UI 优化 |

---

## 最终交付

**阶段 17 — 交付、仓库提交与复盘，已完成。** 全部 17 阶段文档齐全，Playwright 9/9 自动化测试通过，Node.js 脚本烟雾测试 10/10 通过，P0/P1 清零。**最终提交：** `589be72` (`v1.0-final` tag)

### v1.0 后续修复 & 增强

| 项目 | 说明 |
|------|------|
| 我的预约列表白屏修复 | ReservationController.My() 中匿名类型 ViewBag 的 as List\<dynamic\> 因泛型协变失败，添加 (object) 中间转换修复 |
| 导航栏登录按钮不可见修复 | 白色背景 + 白色文字（btn-custom-outline），改为 btn-custom-primary（蓝底白字） |
| 管理员取消不受时段限制 | AdminController 去除管理员取消的时间校验，可强制取消任何 Pending 状态的预约（不限过去/未来）；已取消的预约不可重复取消 |
