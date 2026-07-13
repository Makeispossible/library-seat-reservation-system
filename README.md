# 图书馆座位预约系统

图书馆座位预约系统是一个轻量级 Web 应用，让学生可以通过浏览器在线查看座位空闲情况、预约座位、取消预约；管理员可以在后台管理座位和查看预约记录。系统采用体验账号切换方式，无需注册即可使用。

---

## 技术栈

| 层 | 技术 | 版本 |
|----|------|------|
| 框架 | ASP.NET Core MVC | .NET 9 |
| ORM | Entity Framework Core | 9.x |
| 数据库 | SQL Server LocalDB | 内置 |
| 前端 | Bootstrap | 5.3 (CDN) |
| 前端扩展 | `css/style.css` | 自定义样式 |

> 降级方案：如果 LocalDB 不可用，可切换 SQLite（修改 Program.cs 中 `UseSqlServer` → `UseSqlite`，连接字符串改为 `Data Source=LibrarySeat.db`）。

---

## 目录结构

### 当前已存在 / 本阶段产物

```
LibrarySeatReservation/       ← ASP.NET Core MVC 项目（待创建）
├── docs/                     ← 项目文档
│   ├── 01-项目立项单.md
│   ├── 02-需求分析与MVP确认.md
│   ├── 03-PRD-Lite.md
│   ├── 04-页面树与业务流程.md
│   ├── 05-页面卡与UI规范.md
│   ├── 07-系统设计说明.md
│   ├── 08-数据库设计.md
│   ├── 09-关键链路详细设计.md
│   ├── 09-关键链路详细设计-审计.md
│   ├── 10-开发准备与Sprint0.md      ← 本阶段产物
│   ├── 项目任务板与迭代记录.md        ← 本阶段产物
│   └── superpowers/
│       └── plans/
│           └── 2026-07-13-library-seat-reservation.md
├── prototype/
│   └── static-v1/
│       └── css/
│           └── style.css
├── README.md                          ← 本文件（本阶段产物）
└── opencode.json                      ← OpenCode 配置
```

### 后续计划 / 待生成（编码阶段）

```
LibrarySeatReservation/
├── Controllers/
│   ├── HomeController.cs
│   ├── SeatsController.cs
│   ├── ReservationController.cs
│   └── AdminController.cs
├── Models/
│   ├── StudentUser.cs
│   ├── Seat.cs
│   ├── Reservation.cs
│   ├── SeatDisplay.cs
│   └── ErrorViewModel.cs
├── Data/
│   ├── AppDbContext.cs
│   └── DbInitializer.cs
├── Helpers/
│   └── StatusHelper.cs
├── Filters/
│   └── AdminAuthFilter.cs
├── Views/
│   ├── Home/Index.cshtml
│   ├── Seats/Index.cshtml + Detail.cshtml
│   ├── Reservation/Create.cshtml + My.cshtml
│   ├── Admin/Login.cshtml + Reservations.cshtml + Seats.cshtml + Stats.cshtml
│   ├── Shared/_Layout.cshtml + _AdminLayout.cshtml + _ValidationScriptsPartial.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot/                          ← 静态文件
├── Program.cs
├── appsettings.json
└── LibrarySeatReservation.csproj
```

---

## 运行前提

| 依赖 | 说明 |
|------|------|
| .NET 9 SDK | `dotnet --version` 确认 ≥ 9.0 |
| SQL Server LocalDB | `SqlLocalDB info` 确认可用 |
| 浏览器 | 现代浏览器（Chrome / Edge / Firefox） |

---

## 快速启动

```bash
# 1. 进入项目目录
cd LibrarySeatReservation

# 2. 还原 NuGet 包
dotnet restore

# 3. 首次运行（自动执行迁移 + 种子数据）
dotnet run
```

浏览器访问 `http://localhost:5000` 即可看到首页。

---

## 已实现范围

> 此段落将在编码阶段逐步更新。

| 模块 | 状态 |
|------|------|
| 体验账号切换 | ⏳ 待开发 |
| 座位列表 + 筛选 | ⏳ 待开发 |
| 座位详情 | ⏳ 待开发 |
| 预约提交 + 冲突校验 | ⏳ 待开发 |
| 我的预约 + 取消 | ⏳ 待开发 |
| 管理员登录 | ⏳ 待开发 |
| 座位管理 | ⏳ 待开发 |
| 预约记录查看 | ⏳ 待开发 |
| 统计页 | ⏳ 待开发 |

---

## 数据库初始化方式

首次运行 `dotnet run` 时，`DbInitializer` 自动执行：
1. 检测数据库是否存在，不存在则创建
2. 检测表是否为空，为空则写入种子数据

无需手动执行 `dotnet ef database update`。

---

## 演示账号

| 角色 | 账号 | 说明 |
|------|------|------|
| 学生体验账号 | 学生A ~ 学生E | 首页下拉框切换，无需密码 |
| 管理员 | admin / admin123 | 访问 `/Admin/Login` 登录 |

---

## 已知限制

> 此段落将持续更新已发现但未修复的已知问题。

- 当前无已知限制

---

## 当前阶段

**开发准备与 Sprint 0** — 项目骨架搭建、数据库初始化、首次可运行版本。

下一阶段：Sprint 1 — 用户端首页 + 座位浏览。
