# 图书馆座位预约系统

一个轻量级 Web 应用，让学生可以通过浏览器在线查看座位空闲情况、预约座位、取消预约；管理员可以在后台管理座位和查看预约记录。系统采用体验账号切换方式，无需注册即可使用。

---

## 技术栈

| 层级 | 技术 | 版本 |
|------|------|------|
| 框架 | ASP.NET Core MVC | .NET 9 |
| 对象关系映射 | Entity Framework Core | 9.0.0 |
| 数据库 | SQL Server LocalDB | 内置 |
| 前端 | Bootstrap | 5.3.3 (CDN) |
| 自定义样式 | `wwwroot/css/style.css` | 基于设计规范 |
| 自动化 E2E 测试 | Playwright (`@playwright/test`) | ^1.52.0 |
| 脚本烟雾测试 | Node.js `fetch()` | ≥ 18 |

> 降级方案：如果 LocalDB 不可用，可将 Program.cs 中 `UseSqlServer` 替换为 `UseSqlite`，连接字符串改为 `Data Source=LibrarySeat.db`。

---

## 功能清单

| 模块 | 功能 | 说明 |
|------|------|------|
| 首页 | 体验账号切换 | 下拉框切换 5 个预设学生账号（学生A~学生E），Session 持久化 |
| 座位列表 | 查看全部座位 | 12 座位网格展示，含动态状态（空闲/已预约/使用中/维护中） |
| 座位列表 | 按区域筛选 | 下拉框选择区域（一楼大厅/二楼阅览室/三楼自习区），自动提交 |
| 座位列表 | 按楼层筛选 | 下拉框选择楼层（1F/2F/3F） |
| 座位详情 | 座位基本信息 | 编号、区域、楼层、当前动态状态 |
| 座位详情 | 今日预约时段 | 当日所有预约记录的时间段及状态列表 |
| 预约提交 | 选择时段 | 表单填写开始时间和结束时间 |
| 预约提交 | 时间规则校验 | 结束时间 > 开始时间、最短 30 分钟、最长 4 小时 |
| 预约提交 | 冲突检测 | 同一座位同一时段已有 Pending 预约时拒绝 |
| 预约提交 | 未登录守卫 | 未选择体验账号时跳转首页 |
| 预约提交 | 座位状态守卫 | 非空闲座位不可提交预约 |
| 我的预约 | 预约记录列表 | 当前用户全部预约记录，按日期和时间倒序 |
| 我的预约 | 动态状态显示 | 待开始 / 使用中 / 已完成 / 已取消 |
| 我的预约 | 取消预约 | 仅 Pending 状态且未开始的预约可取消，含确认弹窗 |
| 我的预约 | 空状态 | 无预约记录时显示引导提示 |
| 管理员登录 | 账号密码登录 | admin / admin123，Session 持久化 |
| 管理员登录 | 已登录自动跳转 | 已登录时访问登录页直接跳转预约管理 |
| 管理员登出 | 清除 Session | POST 请求退出，跳转登录页 |
| 预约管理 | 查看全部预约 | 全部预约记录表格，含座位/用户/时间/状态 |
| 预约管理 | 按日期筛选 | 输入日期筛选预约记录 |
| 预约管理 | 按状态筛选 | 按动态状态（待开始/使用中/已完成/已取消）筛选 |
| 预约管理 | 按用户筛选 | 下拉框选择学生账号筛选 |
| 预约管理 | 管理员取消预约 | 管理员可取消任意 Pending 预约（含日期守卫） |
| 座位管理 | 查看全部座位 | 座位表格（ID/编号/区域/楼层/状态） |
| 座位管理 | 添加座位 | 表单填写编号/区域/楼层，含唯一性校验 |
| 座位管理 | 切换状态 | Available ↔ Maintenance，含确认弹窗 |
| 统计页 | 座位总数 | 实时统计 |
| 统计页 | 今日预约数 | 实时统计 |
| 统计页 | 座位使用率 | 今日有 Pending 预约的座位占比，含进度条 |
| 统计页 | 各区域座位分布 | 按区域分组的座位数量表格 |
| 权限守卫 | 管理端 [AdminAuth] | 未登录管理员自动重定向登录页 |
| 权限守卫 | 用户端 Session 检查 | 未选账号时跳转首页提示 |

---

## 页面清单

| 页面 | 路由 | 说明 |
|------|------|------|
| 首页 | `/` 或 `/Home/Index` | 体验账号切换入口 |
| 座位列表 | `/Seats` | 12 座位网格 + 筛选 |
| 座位详情 | `/Seats/Detail/{id}` | 座位信息 + 今日预约时段 |
| 预约表单 | `/Reservation/Create/{id}` | 选择时段提交预约 |
| 我的预约 | `/Reservation/My` | 当前用户的预约记录 |
| 管理员登录 | `/Admin/Login` | 管理员账号密码登录 |
| 预约管理 | `/Admin/Reservations` | 全部预约记录 + 筛选 + 取消 |
| 座位管理 | `/Admin/Seats` | 座位列表 + 添加 + 状态切换 |
| 统计页 | `/Admin/Stats` | 座位使用数据统计 |
| 错误页 | `/Home/Error` | 通用错误页面 |

> 所有管理端页面均受 `[AdminAuth]` 守卫保护。

---

## 运行步骤

### 前置依赖

| 依赖 | 验证命令 |
|------|---------|
| .NET 9 SDK | `dotnet --version` ≥ 9.0 |
| SQL Server LocalDB | `SqlLocalDB info` 显示可用实例 |
| 浏览器 | Chrome / Edge / Firefox |
| Node.js ≥ 18（可选，运行测试用） | `node --version` |

### 首次运行

```bash
# 1. 进入项目目录
cd LibrarySeatReservation

# 2. 还原 NuGet 包
dotnet restore

# 3. 创建数据库（首次需要手动执行迁移）
dotnet ef database update --project src/LibrarySeatReservation.Web

# 4. 运行应用
dotnet run --project src/LibrarySeatReservation.Web
```

浏览器访问 `http://localhost:5211` 即可看到首页。

> 首次启动时，Program.cs 中的 `db.Database.Migrate()` 会自动执行挂起的迁移建表，随后 `DbInitializer.Initialize()` 写入种子数据（检测到表非空则跳过）。

### 每次启动

```bash
dotnet run --project src/LibrarySeatReservation.Web
```

启动后访问 `http://localhost:5211`。

### 运行自动化测试

```powershell
# PowerShell 需先绕过执行策略（仅首次）
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force

# 安装 Playwright 依赖（仅首次）
npm install

# Playwright E2E 自动点击测试（9 用例）
npx playwright test --reporter=list

# Node.js 脚本烟雾测试（10 项）
node scripts/smoke-test.mjs
```

---

## 数据库初始化方式

### 方案：Entity Framework Core Code First + Migrations

项目使用 EF Core Code First 方式，通过 Migration 脚本建表，通过 DbInitializer 写入种子数据。

**数据表结构：**

| 表名 | 说明 | 主要字段 |
|------|------|---------|
| StudentUsers | 体验学生账号 | Id, Name |
| Seats | 座位 | Id, SeatNumber, Area, Floor, Status |
| Reservations | 预约记录 | Id, SeatId, StudentUserId, Date, StartTime, EndTime, Status, CreatedAt |

**索引：**
- `IX_Seats_Area_Floor` — 座位列表按区域/楼层筛选
- `IX_Reservations_SeatId_Date` — 预约冲突检查
- `IX_Reservations_StudentUserId` — 我的预约查询
- `IX_Reservations_Date_StartTime` — 管理端按时间倒序

**首次建库建表：**

```bash
dotnet ef database update --project src/LibrarySeatReservation.Web
```

或者直接 `dotnet run`，Program.cs 中的 `db.Database.Migrate()` 会自动执行。

### 种子数据

种子数据在 `DbInitializer.Initialize()` 中定义，首次启动时自动写入（检测到 StudentUsers 表非空则跳过）。

**5 个体验学生：**

| ID | 姓名 |
|----|------|
| 1 | 学生A |
| 2 | 学生B |
| 3 | 学生C |
| 4 | 学生D |
| 5 | 学生E |

**12 个座位（3 区域 × 4 座位）：**

| 座位编号 | 区域 | 楼层 |
|---------|------|------|
| A-01 ~ A-04 | 一楼大厅 | 1F |
| B-01 ~ B-04 | 二楼阅览室 | 2F |
| C-01 ~ C-04 | 三楼自习区 | 3F |

所有座位初始状态均为 `Available`（可用）。

---

## 演示账号

| 角色 | 账号 | 密码 | 说明 |
|------|------|------|------|
| 学生体验账号 | 学生A / 学生B / 学生C / 学生D / 学生E | 无需密码 | 首页下拉框切换，Session 保持 |
| 管理员 | admin | admin123 | 访问 `/Admin/Login` 登录 |

---

## 项目目录说明

```
LibrarySeatReservation/            ← 仓库根目录
├── docs/                          ← 项目文档（过程记录 + 审计 + 复盘）
│   ├── 01-项目立项单.md           ← 项目章程
│   ├── 02-需求分析与MVP确认.md    ← 需求范围
│   ├── 03-PRD-Lite.md             ← 轻量级 PRD
│   ├── 04-页面树与业务流程.md     ← 页面流 + 业务流程
│   ├── 05-页面卡与UI规范.md       ← UI 设计规范
│   ├── 07-系统设计说明.md         ← 架构 + 路由 + 状态设计
│   ├── 08-数据库设计.md           ← ER 图 + 表结构
│   ├── 09-关键链路详细设计.md     ← 预约/取消/管理员操作详细流程
│   ├── 10-开发准备与Sprint0.md    ← Sprint 0 计划
│   ├── 11-开发前一致性总审计.md   ← 开发前跨文档一致性审计
│   ├── 12-开发起步与骨架记录.md   ← Sprint 0 开发记录
│   ├── 13-用户端主链路开发记录.md ← Sprint 1+2 开发记录
│   ├── 13-用户端主链路开发-审计.md ← Sprint 1+2 审计报告
│   ├── 14-管理端与权限开发记录.md ← Sprint 3 开发记录
│   ├── 14-管理端与权限开发-审计.md ← Sprint 3 审计报告
│   ├── 16-联调测试与缺陷闭环.md   ← Sprint 4 测试报告
│   ├── 17-交付说明与项目复盘.md   ← 最终交付复盘（本文）
│   └── 项目任务板与迭代记录.md    ← 全 Sprint 任务板 + 站会/评审记录
├── database/                      ← 数据库初始化说明
│   └── README.md                  ← 数据库文档（Code First + 种子数据说明）
├── e2e/                           ← Playwright E2E 测试
│   ├── user-flow.spec.ts          ← 用户端主链路（4 用例）
│   └── admin-flow.spec.ts         ← 管理端主链路（5 用例）
├── scripts/
│   └── smoke-test.mjs             ← Node.js 脚本烟雾测试（10 项）
├── src/
│   └── LibrarySeatReservation.Web/ ← ASP.NET Core MVC 项目
│       ├── Controllers/
│       │   ├── HomeController.cs       ← 首页 + 账号切换
│       │   ├── SeatsController.cs      ← 座位列表 + 详情
│       │   ├── ReservationController.cs← 预约 + 我的预约 + 取消
│       │   └── AdminController.cs      ← 管理端全部功能
│       ├── Models/
│       │   ├── StudentUser.cs          ← 体验账号实体
│       │   ├── Seat.cs                 ← 座位实体
│       │   ├── Reservation.cs          ← 预约记录实体
│       │   ├── SeatDisplay.cs          ← 座位列表视图模型
│       │   └── ErrorViewModel.cs       ← 错误页视图模型
│       ├── Data/
│       │   ├── AppDbContext.cs          ← EF Core 数据上下文
│       │   └── DbInitializer.cs        ← 种子数据初始化器
│       ├── Migrations/                 ← EF Core 迁移脚本
│       ├── Helpers/
│       │   └── StatusHelper.cs         ← 动态状态计算工具类
│       ├── Filters/
│       │   └── AdminAuthFilter.cs      ← 管理端认证过滤器
│       ├── Views/                      ← Razor 视图
│       │   ├── Home/Index.cshtml
│       │   ├── Seats/Index.cshtml, Detail.cshtml
│       │   ├── Reservation/Create.cshtml, My.cshtml
│       │   ├── Admin/Login.cshtml, Reservations.cshtml, Seats.cshtml, Stats.cshtml
│       │   └── Shared/_Layout.cshtml, _AdminLayout.cshtml
│       ├── wwwroot/css/style.css       ← 自定义样式表
│       ├── Program.cs                  ← 应用入口 + 中间件管道
│       ├── appsettings.json            ← 连接字符串配置
│       └── LibrarySeatReservation.Web.csproj
├── playwright.config.ts           ← Playwright 配置（msedge + baseURL + webServer）
├── package.json                   ← npm 包管理（Playwright 依赖）
├── LibrarySeatReservation.sln     ← 解决方案文件
└── README.md                      ← 本文
```

---

## 自动化验证

| 测试方式 | 命令 | 覆盖范围 | 最新结果 |
|---------|------|---------|---------|
| Playwright 自动点击 | `npx playwright test` | 用户端 4 用例 + 管理端 5 用例 | ✅ 9/9 通过 |
| Node.js 烟雾测试 | `node scripts/smoke-test.mjs` | 10 项页面可达性检查 | ✅ 10/10 通过 |
| 兼容性测试 | — | Edge + Chrome + 手机竖屏 @media | ✅ 一致 |

---

## 已知限制

> 以下为已发现但未修复的已知问题，不影响主流程演示。

| 编号 | 问题 | 严重度 | 说明 |
|------|------|--------|------|
| TD-01 | `favicon.ico` 缺失（浏览器控制台 404） | P4 | 不影响功能 |
| TD-04 | `LibrarySeatReservation.Web.styles.css` 404 | P4 | ASP.NET Core 可选文件，不影响渲染 |
| TD-05 | 统计页未渲染可用/维护中座位数 | P3 | Controller 已计算 AvailableSeats 和 MaintenanceSeats，但视图未展示这两个指标 |
| TD-06 | 导航栏当前页未高亮 | P3 | CSS 中定义了 `.nav-link.active` 样式，但未通过 JS 或 Razor 根据当前路由动态添加 `active` 类 |

---

## 最终提交

| 项目 | 内容 |
|------|------|
| 最终提交哈希 | `3d7c9db`（Sprint 4 完成后的最新提交） |
| 最终标签 | `v1.0-final` |
| 提交日期 | 2026-07-14 |
