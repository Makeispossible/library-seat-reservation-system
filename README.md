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

```
LibrarySeatReservation/         ← 仓库根目录
├── docs/                       ← 项目文档
│   ├── 01-项目立项单.md
│   ├── 02-需求分析与MVP确认.md
│   ├── 03-PRD-Lite.md
│   ├── 04-页面树与业务流程.md
│   ├── 05-页面卡与UI规范.md
│   ├── 07-系统设计说明.md
│   ├── 08-数据库设计.md
│   ├── 09-关键链路详细设计.md
│   ├── 09-关键链路详细设计-审计.md
│   ├── 10-开发准备与Sprint0.md
│   ├── 11-开发前一致性总审计.md
│   ├── 12-开发起步与骨架记录.md
│   └── 项目任务板与迭代记录.md
├── prototype/
│   └── static-v1/
│       └── css/
│           └── style.css
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

浏览器访问 `http://localhost:5000` 即可看到首页。首次启动时种子数据自动写入。

---

## 已实现范围

| 模块 | 状态 | 说明 |
|------|------|------|
| 项目骨架 | ✅ 已完成 | .sln + .csproj + NuGet + Program.cs + appsettings |
| 数据库 | ✅ 已完成 | EF Core Code First 建 3 表 + 4 索引 + 种子数据 |
| 体验账号切换 | 🔲 Controller+View 就绪 | 需 Sprint 1 dotnet run 验证 |
| 座位列表 + 筛选 | 🔲 Controller+View 就绪 | 需 Sprint 1 数据验证 |
| 座位详情 + 时段 | 🔲 Controller+View 就绪 | 需 Sprint 1 验证 |
| 预约提交 + 冲突校验 | 🔲 Controller+View 就绪 | 需 Sprint 2 验证 |
| 我的预约 + 取消 | 🔲 Controller+View 就绪 | 需 Sprint 2 验证 |
| 管理员登录 | 🔲 Controller+View 就绪 | 需 Sprint 3 验证 |
| 座位管理 | 🔲 View 就绪 | 需 Sprint 3 集成验证 |
| 预约记录查看 | 🔲 View 就绪 | 需 Sprint 3 集成验证 |
| 统计页 | 🔲 View 就绪 | 需 Sprint 3 集成验证 |

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

- 当前无已知限制

---

## 当前阶段

**Sprint 0 — 开发起步与项目骨架，已完成。** 所有 Controller、View、Model 源码已就绪，数据库已建库建表，构建零错误。

下一阶段：Sprint 1 — 用户端首页 + 座位列表 + 座位详情。（`dotnet run` 验证 + 数据脱敏）
