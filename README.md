# 图书馆座位预约系统

一个基于 **ASP.NET Core MVC + SQL Server LocalDB** 的《.NET网络开发技术》课程设计项目，用于在线查看座位空闲情况、预约座位、取消预约和后台管理。系统采用体验账号切换方式，无需注册即可使用。

## 项目简介

系统围绕高校图书馆座位预约场景设计，覆盖学生浏览座位、预约提交、取消预约和管理员后台管理的完整业务闭环。项目首次运行会自动创建数据库并写入演示数据，便于课程答辩和功能展示。

## 功能模块

- **体验账号切换**：无需注册，下拉框一键切换 5 个预设学生账号（学生A~学生E），Session 持久化
- **座位浏览**：12 座位网格展示，支持按区域（一楼大厅/二楼阅览室/三楼自习区）和楼层（1F/2F/3F）筛选
- **动态状态**：座位展示实时状态——空闲、已预约、使用中、维护中
- **预约提交**：选择时段提交预约，含时间规则校验（最短 30 分钟、最长 4 小时）、冲突检测、多重守卫
- **我的预约**：查看当前用户的全部预约记录，含动态状态（待开始/使用中/已完成/已取消），可取消未开始的预约
- **管理员登录**：admin / admin123 登录，独立 Session 管理
- **预约管理**：管理员查看全部预约记录，支持按日期、状态、用户筛选，可取消任意 Pending 预约
- **座位管理**：管理员添加新座位（唯一性校验）、切换座位可用/维护状态
- **数据统计**：实时计算座位总数、今日预约数、座位使用率、各区域座位分布
- **权限控制**：管理端 [AdminAuth] 过滤器守卫，用户端 Session 检查

## 技术栈

| 类型 | 技术 |
|------|------|
| 后端框架 | ASP.NET Core MVC |
| 目标框架 | .NET 9 |
| 编程语言 | C#、Razor、HTML、CSS |
| 数据库 | SQL Server LocalDB |
| 数据访问 | Entity Framework Core 9.0 |
| 前端样式 | Bootstrap 5.3 (CDN) + 自定义 CSS |
| 自动化测试 | Playwright (msedge) + Node.js 脚本烟雾测试 |

## 项目结构

```
LibrarySeatReservation
├── Controllers              # MVC 控制器
│   ├── HomeController.cs        # 首页 + 账号切换
│   ├── SeatsController.cs       # 座位列表 + 详情
│   ├── ReservationController.cs # 预约 + 我的预约 + 取消
│   └── AdminController.cs       # 管理端全部功能
├── Data                     # 数据库上下文与初始化
│   ├── AppDbContext.cs
│   └── DbInitializer.cs
├── Database                 # 数据库初始化说明
│   └── README.md
├── Docs                     # 课程设计文档
│   ├── 项目文档.md
│   └── 软件配置与使用说明.md
├── Filters                  # 过滤器
│   └── AdminAuthFilter.cs       # 管理端认证
├── Helpers                  # 工具类
│   └── StatusHelper.cs          # 动态状态计算
├── Migrations               # EF Core 迁移脚本
├── Models                   # 数据模型与视图模型
│   ├── StudentUser.cs
│   ├── Seat.cs
│   ├── Reservation.cs
│   ├── SeatDisplay.cs
│   └── ErrorViewModel.cs
├── Properties               # 启动配置
│   └── launchSettings.json
├── Views                    # Razor 视图页面
│   ├── Home/                    # 首页
│   ├── Seats/                   # 座位列表 + 详情
│   ├── Reservation/             # 预约表单 + 我的预约
│   ├── Admin/                   # 管理端页面
│   └── Shared/                  # 布局模板
├── wwwroot                  # 静态资源
│   └── css/style.css
├── e2e                      # Playwright E2E 测试
│   ├── user-flow.spec.ts        # 用户端 4 用例
│   └── admin-flow.spec.ts       # 管理端 5 用例
├── scripts                  # 辅助脚本
│   └── smoke-test.mjs           # Node.js 烟雾测试
├── Program.cs               # 应用入口
├── appsettings.json         # 连接字符串配置
├── LibrarySeatReservation.Web.csproj
├── LibrarySeatReservation.sln
└── README.md
```

## 运行环境

- Windows 10/11、Linux 或 macOS
- .NET SDK 9.0 或更高版本
- SQL Server LocalDB（Windows 自带）
- Edge、Chrome 或 Firefox 浏览器
- Node.js ≥ 18（可选，运行测试用）

## 快速开始

进入项目目录：

```bash
cd library-seat-reservation-system
```

还原依赖：

```bash
dotnet restore
```

启动系统：

```bash
dotnet run
```

浏览器打开：

```
http://localhost:5211
```

首次启动会自动创建数据库、建表并写入种子数据。

> 降级方案：如果没有 SQL Server LocalDB，可将 `Program.cs` 中的 `UseSqlServer` 替换为 `UseSqlite("Data Source=LibrarySeat.db")`。

## 登录账号

| 角色 | 账号 | 密码 | 说明 |
|------|------|------|------|
| 学生体验 | 学生A / 学生B / 学生C / 学生D / 学生E | 无 | 首页下拉框切换 |
| 管理员 | admin | admin123 | 访问 `/Admin/Login` 登录 |

## 运行测试

```powershell
# 安装 Playwright 依赖（仅首次）
npm install

# Playwright E2E 自动测试（9 用例）
npx playwright test --reporter=list

# Node.js 脚本烟雾测试（10 项）
node scripts/smoke-test.mjs
```

## 默认数据

系统首次运行自动创建数据库并写入：

- **5 个体验学生**：学生A ~ 学生E
- **12 个座位**：A-01~A-04（一楼大厅）、B-01~B-04（二楼阅览室）、C-01~C-04（三楼自习区），全部初始状态为 Available

如需重置数据：

```bash
dotnet ef database drop
dotnet run
```

## 课程设计提交内容

- 完整 ASP.NET Core MVC 源码
- SQL Server LocalDB 数据库（首次运行自动创建）
- Entity Framework Core 迁移脚本
- 软件配置与使用说明
- 项目文档

## 说明

本项目用于课程设计学习与展示。
