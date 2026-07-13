# 图书馆座位预约系统 — 完整编码实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task.

**Goal:** 从零创建可运行的 ASP.NET Core MVC 图书馆座位预约系统（9 页面、EF Core + LocalDB、Session 认证、动态状态计算）

**Architecture:** 标准 ASP.NET Core MVC (.NET 9)，EF Core 连接 SQL Server LocalDB，Bootstrap 5 前端，Session 管理端认证，动态状态计算（不依赖后台 Worker）

**Tech Stack:** .NET 9, ASP.NET Core MVC, EF Core 9 + SQL Server LocalDB, Bootstrap 5, Razor

---

## 全局约束

- .NET 9 SDK 已安装（`dotnet --version` → 9.0.200）
- SQL Server LocalDB 可用（`(localdb)\MSSQLLocalDB`）
- 项目路径：`D:\AIWeb\LibrarySeatReservation`（新建项目，清理旧的 SecondClassroomManager）
- 不删除 docs/ 目录下的任何文档
- 所有 View 使用 Razor + Bootstrap 5，不写 JavaScript（Bootstrap 自带组件足够）
- 管理端认证使用 Session，不做 Identity Framework
- 动态状态计算使用服务端逻辑，不依赖 SignalR / 定时任务
- 数据库表名：`StudentUsers`, `Seats`, `Reservations`

---

## 文件结构

```
D:\AIWeb\LibrarySeatReservation\
├── Controllers\
│   ├── HomeController.cs          # 用户首页 + 切换体验账号
│   ├── SeatsController.cs         # 座位列表 + 座位详情
│   ├── ReservationController.cs   # 提交预约 + 我的预约 + 取消
│   └── AdminController.cs         # 登录/登出 + 管理预约/座位/统计
├── Models\
│   ├── StudentUser.cs             # 体验账号实体
│   ├── Seat.cs                    # 座位实体
│   ├── Reservation.cs             # 预约记录实体
│   ├── SeatDisplay.cs             # 视图模型：带显示状态的座位
│   └── ErrorViewModel.cs         # 默认错误模型
├── Data\
│   ├── AppDbContext.cs            # EF Core 数据上下文
│   └── DbInitializer.cs          # 种子数据初始化
├── Filters\
│   └── AdminAuthFilter.cs         # 管理端 Session 认证过滤器
├── Helpers\
│   └── StatusHelper.cs            # 动态状态计算静态方法
├── Views\
│   ├── Home\
│   │   └── Index.cshtml           # 用户首页
│   ├── Seats\
│   │   ├── Index.cshtml           # 座位列表页
│   │   └── Detail.cshtml          # 座位详情页
│   ├── Reservation\
│   │   ├── Create.cshtml          # 预约提交页
│   │   └── My.cshtml              # 我的预约页
│   ├── Admin\
│   │   ├── Login.cshtml           # 管理员登录页
│   │   ├── Reservations.cshtml    # 预约管理页
│   │   ├── Seats.cshtml           # 座位管理页
│   │   └── Stats.cshtml           # 统计页
│   ├── Shared\
│   │   ├── _Layout.cshtml         # 用户端布局（蓝色导航栏）
│   │   ├── _AdminLayout.cshtml    # 管理端布局（深色导航栏）
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── _ViewImports.cshtml
│   └── _ViewStart.cshtml
├── wwwroot\                       # 静态文件（Bootstrap 内置）
├── Program.cs                     # 应用入口 + 服务注册
├── appsettings.json               # 连接字符串 + 配置
└── LibrarySeatReservation.csproj  # 项目文件（net9.0）

共计：约 30 个文件
```

---

## 核心数据模型

### StudentUser（体验账号）

```csharp
public class StudentUser
{
    public int Id { get; set; }
    public string Name { get; set; } = "";      // "学生A" ~ "学生E"
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
```

### Seat（座位）

```csharp
public class Seat
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = "";  // 编号，如 "A-01"
    public string Area { get; set; } = "";        // 区域，如 "一楼大厅"
    public string Floor { get; set; } = "";       // 楼层，如 "1F"
    public string Status { get; set; } = "Available";  // 持久化状态: Available / Maintenance
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
```

### Reservation（预约记录）

```csharp
public class Reservation
{
    public int Id { get; set; }
    public int SeatId { get; set; }
    public int StudentUserId { get; set; }
    public DateTime Date { get; set; }            // 预约日期（服务器当天）
    public TimeSpan StartTime { get; set; }        // 开始时间
    public TimeSpan EndTime { get; set; }          // 结束时间
    public string Status { get; set; } = "Pending"; // 持久化状态: Pending / Cancelled
    public DateTime CreatedAt { get; set; }
    public Seat Seat { get; set; } = null!;
    public StudentUser StudentUser { get; set; } = null!;
}
```

### SeatDisplay（视图模型）

```csharp
public class SeatDisplay
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = "";
    public string Area { get; set; } = "";
    public string Floor { get; set; } = "";
    public string DisplayStatus { get; set; } = ""; // "空闲"/"已预约"/"使用中"/"维护中"
}
```

---

## 任务分解

---
### Task 1: 创建项目 + NuGet 包 + 配置

**文件：**
- 创建：`D:\AIWeb\LibrarySeatReservation\LibrarySeatReservation.csproj`
- 创建：`D:\AIWeb\LibrarySeatReservation\Program.cs`
- 创建：`D:\AIWeb\LibrarySeatReservation\appsettings.json`

**接口：**
- 产出：可运行的空 MVC 项目，配置好 EF Core + Session

- [ ] **Step 1: 创建新项目**

```bash
# 删除旧的模板项目目录
Remove-Item -Recurse -Force "D:\AIWeb\SecondClassroomManager"
# 创建新项目
cd D:\AIWeb
dotnet new mvc -n LibrarySeatReservation --no-https
cd LibrarySeatReservation
# 添加 EF Core SQL Server 包
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

验证：`dotnet build` 应无错误

- [ ] **Step 2: 配置 appsettings.json**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LibrarySeatReservation;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

- [ ] **Step 3: 配置 Program.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using LibrarySeatReservation.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session（用于管理端认证和体验账号切换）
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 初始化种子数据
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    DbInitializer.Initialize(context);
}

app.Run();
```

- [ ] **Step 4: 验证构建通过**

```bash
cd D:\AIWeb\LibrarySeatReservation
dotnet build
```

预期：Build succeeded

---
### Task 2: 数据模型 + DbContext + 种子数据

**文件：**
- 创建：`D:\AIWeb\LibrarySeatReservation\Models\StudentUser.cs`
- 创建：`D:\AIWeb\LibrarySeatReservation\Models\Seat.cs`
- 创建：`D:\AIWeb\LibrarySeatReservation\Models\Reservation.cs`
- 创建：`D:\AIWeb\LibrarySeatReservation\Models\SeatDisplay.cs`
- 创建：`D:\AIWeb\LibrarySeatReservation\Data\AppDbContext.cs`
- 创建：`D:\AIWeb\LibrarySeatReservation\Data\DbInitializer.cs`

**接口：**
- 产出：完整的数据层，可创建数据库和种子数据
- 下游：Task 3+ 通过 `AppDbContext` 操作数据库

- [ ] **Step 1: 创建 Model 目录和实体文件**

```bash
mkdir D:\AIWeb\LibrarySeatReservation\Models
mkdir D:\AIWeb\LibrarySeatReservation\Data
```

- [ ] **Step 2: StudentUser.cs**

```csharp
namespace LibrarySeatReservation.Models;

public class StudentUser
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
```

- [ ] **Step 3: Seat.cs**

```csharp
namespace LibrarySeatReservation.Models;

public class Seat
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = "";
    public string Area { get; set; } = "";
    public string Floor { get; set; } = "";
    public string Status { get; set; } = "Available"; // Available / Maintenance

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
```

- [ ] **Step 4: Reservation.cs**

```csharp
namespace LibrarySeatReservation.Models;

public class Reservation
{
    public int Id { get; set; }
    public int SeatId { get; set; }
    public int StudentUserId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = "Pending"; // Pending / Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Seat Seat { get; set; } = null!;
    public StudentUser StudentUser { get; set; } = null!;
}
```

- [ ] **Step 5: SeatDisplay.cs（视图模型）**

```csharp
namespace LibrarySeatReservation.Models;

public class SeatDisplay
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = "";
    public string Area { get; set; } = "";
    public string Floor { get; set; } = "";
    public string DisplayStatus { get; set; } = ""; // 空闲/已预约/使用中/维护中
}
```

- [ ] **Step 6: AppDbContext.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using LibrarySeatReservation.Models;

namespace LibrarySeatReservation.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<StudentUser> StudentUsers => Set<StudentUser>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StudentUser>(entity =>
        {
            entity.ToTable("StudentUsers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.ToTable("Seats");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SeatNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Area).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Floor).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Seat)
                  .WithMany(s => s.Reservations)
                  .HasForeignKey(e => e.SeatId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.StudentUser)
                  .WithMany(u => u.Reservations)
                  .HasForeignKey(e => e.StudentUserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

- [ ] **Step 7: DbInitializer.cs**

```csharp
using LibrarySeatReservation.Models;

namespace LibrarySeatReservation.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        if (context.StudentUsers.Any()) return; // 已初始化

        // 5 个体验账号
        var students = new List<StudentUser>
        {
            new() { Name = "学生A" },
            new() { Name = "学生B" },
            new() { Name = "学生C" },
            new() { Name = "学生D" },
            new() { Name = "学生E" },
        };
        context.StudentUsers.AddRange(students);

        // 12 个座位（3 个区域 × 4 个座位）
        var seats = new List<Seat>
        {
            // 一楼大厅
            new() { SeatNumber = "A-01", Area = "一楼大厅", Floor = "1F", Status = "Available" },
            new() { SeatNumber = "A-02", Area = "一楼大厅", Floor = "1F", Status = "Available" },
            new() { SeatNumber = "A-03", Area = "一楼大厅", Floor = "1F", Status = "Available" },
            new() { SeatNumber = "A-04", Area = "一楼大厅", Floor = "1F", Status = "Available" },
            // 二楼阅览室
            new() { SeatNumber = "B-01", Area = "二楼阅览室", Floor = "2F", Status = "Available" },
            new() { SeatNumber = "B-02", Area = "二楼阅览室", Floor = "2F", Status = "Available" },
            new() { SeatNumber = "B-03", Area = "二楼阅览室", Floor = "2F", Status = "Available" },
            new() { SeatNumber = "B-04", Area = "二楼阅览室", Floor = "2F", Status = "Available" },
            // 三楼自习区
            new() { SeatNumber = "C-01", Area = "三楼自习区", Floor = "3F", Status = "Available" },
            new() { SeatNumber = "C-02", Area = "三楼自习区", Floor = "3F", Status = "Available" },
            new() { SeatNumber = "C-03", Area = "三楼自习区", Floor = "3F", Status = "Available" },
            new() { SeatNumber = "C-04", Area = "三楼自习区", Floor = "3F", Status = "Available" },
        };
        context.Seats.AddRange(seats);

        context.SaveChanges();
    }
}
```

- [ ] **Step 8: 验证构建通过并创建数据库**

```bash
cd D:\AIWeb\LibrarySeatReservation
dotnet build
# 运行一次让 EnsureCreated 创建数据库
dotnet run &
Start-Sleep -Seconds 5
# 停止
Stop-Process -Name "dotnet" -ErrorAction SilentlyContinue
```

注意：第一次运行会自动创建数据库和种子数据。可以用 VS 的 SQL Server Object Explorer 或 `sqlcmd` 验证。

---
### Task 3: 动态状态计算 + AdminAuthFilter

**文件：**
- 创建：`D:\AIWeb\LibrarySeatReservation\Helpers\StatusHelper.cs`
- 创建：`D:\AIWeb\LibrarySeatReservation\Filters\AdminAuthFilter.cs`

**接口：**
- 产出：其他 Task 可调用的状态计算方法和认证过滤器
- `StatusHelper.GetSeatDisplayStatus(seatStatus, todayReservations)` → "空闲"/"已预约"/"使用中"/"维护中"
- `StatusHelper.GetReservationDisplayStatus(reservationStatus, startTime, endTime)` → "待开始"/"使用中"/"已完成"/"已取消"
- `[AdminAuth]` ActionFilter → 未登录跳转 /Admin/Login

- [ ] **Step 1: 创建目录**

```bash
mkdir D:\AIWeb\LibrarySeatReservation\Helpers
mkdir D:\AIWeb\LibrarySeatReservation\Filters
```

- [ ] **Step 2: StatusHelper.cs**

```csharp
using LibrarySeatReservation.Models;

namespace LibrarySeatReservation.Helpers;

public static class StatusHelper
{
    /// <summary>
    /// 计算座位的显示状态（动态计算）
    /// </summary>
    public static string GetSeatDisplayStatus(string dbStatus, List<Reservation> todayReservations)
    {
        if (dbStatus == "Maintenance")
            return "维护中";

        var now = DateTime.Now.TimeOfDay;
        var today = DateTime.Today;

        // 找出当前时间范围内的预约（使用中）
        var active = todayReservations.Find(r =>
            r.Status == "Pending" &&
            r.StartTime <= now &&
            r.EndTime > now);

        if (active != null)
            return "使用中";

        // 找出未来时间段的预约（已预约）
        var upcoming = todayReservations.Find(r =>
            r.Status == "Pending" &&
            r.StartTime > now);

        if (upcoming != null)
            return "已预约";

        return "空闲";
    }

    /// <summary>
    /// 计算预约记录的显示状态（动态计算）
    /// </summary>
    public static string GetReservationDisplayStatus(string dbStatus, TimeSpan startTime, TimeSpan endTime)
    {
        if (dbStatus == "Cancelled")
            return "已取消";

        var now = DateTime.Now.TimeOfDay;

        if (now < startTime)
            return "待开始";

        if (now >= startTime && now < endTime)
            return "使用中";

        return "已完成";
    }
}
```

- [ ] **Step 3: AdminAuthFilter.cs**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LibrarySeatReservation.Filters;

public class AdminAuthAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Session.GetString("AdminLoggedIn") != "true")
        {
            context.Result = new RedirectToActionResult("Login", "Admin", null);
        }
    }
}
```

- [ ] **Step 4: 验证构建通过**

```bash
cd D:\AIWeb\LibrarySeatReservation
dotnet build
```

---
### Task 4: Layout + 导航栏（Bootstrap 5 前端框架）

**文件：**
- 修改：`D:\AIWeb\LibrarySeatReservation\Views\Shared\_Layout.cshtml`
- 创建：`D:\AIWeb\LibrarySeatReservation\Views\Shared\_AdminLayout.cshtml`
- 修改：`D:\AIWeb\LibrarySeatReservation\Views\_ViewImports.cshtml`
- 修改：`D:\AIWeb\LibrarySeatReservation\Views\_ViewStart.cshtml`

**接口：**
- 产出：用户端和管理端两套布局，后续 Task 的 View 直接使用

- [ ] **Step 1: _ViewImports.cshtml**

```cshtml
@using LibrarySeatReservation
@using LibrarySeatReservation.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

- [ ] **Step 2: _ViewStart.cshtml**

```cshtml
@{
    Layout = "_Layout";
}
```

- [ ] **Step 3: _Layout.cshtml（用户端 - 蓝色导航栏）**

```cshtml
<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - 图书馆座位预约</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
    <style>
        body {
            background-color: #f8fafc;
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
        }
        .navbar-user {
            background-color: #2563eb !important;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }
        .navbar-user .nav-link {
            color: rgba(255,255,255,0.9) !important;
        }
        .navbar-user .nav-link:hover,
        .navbar-user .nav-link.active {
            color: #fff !important;
        }
        .navbar-user .navbar-brand {
            color: #fff !important;
            font-weight: 600;
        }
        .user-selector {
            background: rgba(255,255,255,0.15);
            color: #fff;
            border: 1px solid rgba(255,255,255,0.3);
            border-radius: 6px;
            padding: 4px 8px;
            font-size: 14px;
        }
        .user-selector option {
            color: #1e293b;
            background: #fff;
        }
        .page-container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px 16px;
        }
        .seat-card {
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            padding: 16px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.06);
            cursor: pointer;
            transition: box-shadow 0.15s;
        }
        .seat-card:hover {
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }
        .status-badge {
            display: inline-block;
            padding: 2px 10px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: 500;
        }
        .status-available { background: #dcfce7; color: #166534; }
        .status-reserved  { background: #fff7ed; color: #9a3412; }
        .status-inuse     { background: #dbeafe; color: #1e40af; }
        .status-maintenance { background: #f1f5f9; color: #475569; }
        .status-pending   { background: #dbeafe; color: #1e40af; }
        .status-completed { background: #f0fdf4; color: #166534; }
        .status-cancelled { background: #fef2f2; color: #991b1b; }
        .btn-primary-custom {
            background-color: #2563eb;
            border-color: #2563eb;
            border-radius: 8px;
            padding: 10px 24px;
        }
        .btn-primary-custom:hover {
            background-color: #1d4ed8;
        }
        .btn-danger-custom {
            border-radius: 8px;
            padding: 6px 16px;
        }
        .card-info {
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            padding: 20px;
        }
        footer {
            text-align: center;
            padding: 20px;
            color: #94a3b8;
            font-size: 13px;
        }
    </style>
</head>
<body>
    <nav class="navbar navbar-expand-sm navbar-user mb-3">
        <div class="container">
            <a class="navbar-brand" href="/">📚 座位预约</a>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav me-auto">
                    <li class="nav-item">
                        <a class="nav-link" asp-controller="Home" asp-action="Index">首页</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" asp-controller="Seats" asp-action="Index">座位列表</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" asp-controller="Reservation" asp-action="My">我的预约</a>
                    </li>
                </ul>
                <form class="d-flex align-items-center gap-2" method="post" asp-controller="Home" asp-action="SwitchUser">
                    <span class="text-white" style="font-size:14px;opacity:0.9;">
                        当前：@(ViewBag.CurrentUser ?? "未选择")
                    </span>
                    <select name="userId" class="user-selector" onchange="this.form.submit()">
                        <option value="">切换账号...</option>
                        @if (ViewBag.StudentUsers is List<StudentUser> users)
                        {
                            @foreach (var u in users)
                            {
                                <option value="@u.Id">@u.Name</option>
                            }
                        }
                    </select>
                </form>
            </div>
        </div>
    </nav>

    <div class="page-container">
        @if (TempData["SuccessMessage"] != null)
        {
            <div class="alert alert-success alert-dismissible fade show">@TempData["SuccessMessage"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        @if (TempData["ErrorMessage"] != null)
        {
            <div class="alert alert-danger alert-dismissible fade show">@TempData["ErrorMessage"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        @RenderBody()
    </div>

    <footer>图书馆座位预约系统 — ASP.NET Core MVC</footer>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

- [ ] **Step 4: _AdminLayout.cshtml（管理端 - 深色导航栏）**

```cshtml
@{
    Layout = null;  // 独立布局，不继承 _Layout
}

<!DOCTYPE html>
<html lang="zh-CN">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - 管理后台</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" />
    <style>
        body {
            background-color: #f8fafc;
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
        }
        .navbar-admin {
            background-color: #1e293b !important;
        }
        .navbar-admin .nav-link {
            color: rgba(255,255,255,0.85) !important;
        }
        .navbar-admin .nav-link:hover,
        .navbar-admin .nav-link.active {
            color: #fff !important;
        }
        .navbar-admin .navbar-brand {
            color: #fff !important;
            font-weight: 600;
        }
        .admin-container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px 16px;
        }
        .table-admin th {
            background: #f1f5f9;
            color: #475569;
            font-size: 13px;
            font-weight: 600;
        }
        .table-admin td {
            vertical-align: middle;
        }
        .stats-card {
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 12px;
            padding: 24px;
            text-align: center;
        }
        .stats-number {
            font-size: 32px;
            font-weight: 700;
            color: #2563eb;
        }
        .stats-label {
            color: #64748b;
            font-size: 14px;
        }
        .status-badge {
            display: inline-block;
            padding: 2px 10px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: 500;
        }
        .status-available { background: #dcfce7; color: #166534; }
        .status-reserved  { background: #fff7ed; color: #9a3412; }
        .status-inuse     { background: #dbeafe; color: #1e40af; }
        .status-maintenance { background: #f1f5f9; color: #475569; }
        .status-pending   { background: #dbeafe; color: #1e40af; }
        .status-completed { background: #f0fdf4; color: #166534; }
        .status-cancelled { background: #fef2f2; color: #991b1b; }
    </style>
</head>
<body>
    <nav class="navbar navbar-expand-sm navbar-admin mb-3">
        <div class="container">
            <a class="navbar-brand" href="/Admin/Reservations">⚙️ 管理后台</a>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#adminNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="adminNav">
                <ul class="navbar-nav me-auto">
                    <li class="nav-item">
                        <a class="nav-link" asp-controller="Admin" asp-action="Reservations">预约管理</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" asp-controller="Admin" asp-action="Seats">座位管理</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" asp-controller="Admin" asp-action="Stats">统计</a>
                    </li>
                </ul>
                <form method="post" asp-controller="Admin" asp-action="Logout">
                    <button type="submit" class="btn btn-outline-light btn-sm">退出</button>
                </form>
            </div>
        </div>
    </nav>

    <div class="admin-container">
        @if (TempData["SuccessMessage"] != null)
        {
            <div class="alert alert-success alert-dismissible fade show">@TempData["SuccessMessage"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        @if (TempData["ErrorMessage"] != null)
        {
            <div class="alert alert-danger alert-dismissible fade show">@TempData["ErrorMessage"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        @RenderBody()
    </div>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

- [ ] **Step 5: 验证构建**

```bash
cd D:\AIWeb\LibrarySeatReservation
dotnet build
```

---
### Task 5: HomeController — 用户首页 + 切换账号

**文件：**
- 创建：`D:\AIWeb\LibrarySeatReservation\Controllers\HomeController.cs`
- 创建：`D:\AIWeb\LibrarySeatReservation\Views\Home\Index.cshtml`

**依赖：** Task 2（数据模型）、Task 4（Layout）

- [ ] **Step 1: HomeController.cs**

```csharp
using Microsoft.AspNetCore.Mvc;
using LibrarySeatReservation.Data;

namespace LibrarySeatReservation.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        ViewBag.StudentUsers = _db.StudentUsers.OrderBy(u => u.Id).ToList();
        SetCurrentUser();
        return View();
    }

    [HttpPost]
    public IActionResult SwitchUser(int userId)
    {
        var user = _db.StudentUsers.Find(userId);
        if (user != null)
        {
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
        }
        return RedirectToAction("Index");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("UserId");
        HttpContext.Session.Remove("UserName");
        return RedirectToAction("Index");
    }

    private void SetCurrentUser()
    {
        var name = HttpContext.Session.GetString("UserName");
        ViewBag.CurrentUser = name ?? "未选择";
        ViewBag.StudentUsers = _db.StudentUsers.OrderBy(u => u.Id).ToList();
    }
}
```

- [ ] **Step 2: Views/Home/Index.cshtml**

```cshtml
@{
    ViewData["Title"] = "首页";
}

<div class="text-center py-5">
    <h1 class="display-6 fw-bold mb-3" style="color:#1e293b;">图书馆座位预约系统</h1>
    <p class="text-secondary mb-4">选择体验账号，快速预约图书馆座位</p>

    @if (ViewBag.CurrentUser != "未选择")
    {
        <div class="card-info mx-auto mb-4" style="max-width:400px;">
            <p class="mb-2">当前已选择：<strong>@ViewBag.CurrentUser</strong></p>
            <div class="d-flex gap-3 justify-content-center">
                <a class="btn btn-primary btn-primary-custom" asp-controller="Seats" asp-action="Index">
                    查看座位列表
                </a>
                <a class="btn btn-outline-primary" asp-controller="Reservation" asp-action="My">
                    我的预约
                </a>
            </div>
        </div>
    }
    else
    {
        <div class="card-info mx-auto" style="max-width:400px;">
            <p class="text-muted mb-3">请先在顶部导航栏选择一个体验账号</p>
            <p class="text-muted small">顶部导航右侧下拉框 → 选择 学生A ~ 学生E</p>
        </div>
    }
</div>
```

- [ ] **Step 3: 验证构建**

```bash
cd D:\AIWeb\LibrarySeatReservation
dotnet build
```

---
### Task 6: SeatsController — 座位列表 + 座位详情

**文件：**
- 创建：`D:\AIWeb\LibrarySeatReservation\Controllers\SeatsController.cs`
- 创建：`D:\AIWeb\LibrarySeatReservation\Views\Seats\Index.cshtml`
- 创建：`D:\AIWeb\LibrarySeatReservation\Views\Seats\Detail.cshtml`

**依赖：** Task 2（数据模型）、Task 3（StatusHelper）、Task 4（Layout）

- [ ] **Step 1: SeatsController.cs**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySeatReservation.Data;
using LibrarySeatReservation.Helpers;

namespace LibrarySeatReservation.Controllers;

public class SeatsController : Controller
{
    private readonly AppDbContext _db;

    public SeatsController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index(string? area, string? floor)
    {
        var now = DateTime.Now;
        var today = now.Date;
        var timeOfDay = now.TimeOfDay;

        var query = _db.Seats.AsQueryable();
        if (!string.IsNullOrEmpty(area)) query = query.Where(s => s.Area == area);
        if (!string.IsNullOrEmpty(floor)) query = query.Where(s => s.Floor == floor);

        var seats = query.OrderBy(s => s.SeatNumber).ToList();

        // 获取今天所有预约记录
        var todayReservations = _db.Reservations
            .Where(r => r.Date == today && r.Status == "Pending")
            .ToList();

        var displayList = seats.Select(s =>
        {
            var seatReservations = todayReservations.Where(r => r.SeatId == s.Id).ToList();
            return new SeatDisplay
            {
                Id = s.Id,
                SeatNumber = s.SeatNumber,
                Area = s.Area,
                Floor = s.Floor,
                DisplayStatus = StatusHelper.GetSeatDisplayStatus(s.Status, seatReservations)
            };
        }).ToList();

        ViewBag.Areas = _db.Seats.Select(s => s.Area).Distinct().OrderBy(a => a).ToList();
        ViewBag.Floors = _db.Seats.Select(s => s.Floor).Distinct().OrderBy(f => f).ToList();
        ViewBag.SelectedArea = area;
        ViewBag.SelectedFloor = floor;

        SetUserContext();
        return View(displayList);
    }

    public IActionResult Detail(int id)
    {
        var seat = _db.Seats.FirstOrDefault(s => s.Id == id);
        if (seat == null)
        {
            return NotFound();
        }

        var today = DateTime.Today;
        var reservations = _db.Reservations
            .Include(r => r.StudentUser)
            .Where(r => r.SeatId == id && r.Date == today)
            .OrderBy(r => r.StartTime)
            .ToList();

        // 为每个预约计算显示状态
        var displayReservations = reservations.Select(r => new
        {
            r.Id,
            UserName = r.StudentUser.Name,
            r.StartTime,
            r.EndTime,
            Status = StatusHelper.GetReservationDisplayStatus(r.Status, r.StartTime, r.EndTime)
        }).ToList();

        var seatReservations = reservations.Where(r => r.Status == "Pending").ToList();
        var displayStatus = StatusHelper.GetSeatDisplayStatus(seat.Status, seatReservations);

        ViewBag.DisplayStatus = displayStatus;
        ViewBag.Reservations = displayReservations;

        SetUserContext();
        return View(seat);
    }

    private void SetUserContext()
    {
        ViewBag.CurrentUser = HttpContext.Session.GetString("UserName") ?? "未选择";
        ViewBag.StudentUsers = _db.StudentUsers.OrderBy(u => u.Id).ToList();
    }
}
```

- [ ] **Step 2: Views/Seats/Index.cshtml**

```cshtml
@model List<SeatDisplay>
@{
    ViewData["Title"] = "座位列表";
}

<h4 class="mb-3 fw-bold">座位列表</h4>

<!-- 筛选区 -->
<form method="get" class="row g-2 mb-4">
    <div class="col-auto">
        <select name="area" class="form-select" onchange="this.form.submit()">
            <option value="">全部区域</option>
            @foreach (var a in ViewBag.Areas)
            {
                <option value="@a" selected="@(ViewBag.SelectedArea == a ? true : null)">@a</option>
            }
        </select>
    </div>
    <div class="col-auto">
        <select name="floor" class="form-select" onchange="this.form.submit()">
            <option value="">全部楼层</option>
            @foreach (var f in ViewBag.Floors)
            {
                <option value="@f" selected="@(ViewBag.SelectedFloor == f ? true : null)">@f</option>
            }
        </select>
    </div>
    <div class="col-auto">
        <a href="/Seats" class="btn btn-outline-secondary">清除筛选</a>
    </div>
</form>

@if (!Model.Any())
{
    <div class="text-center py-5 text-muted">该区域暂无座位</div>
}
else
{
    <div class="row g-3">
        @foreach (var seat in Model)
        {
            <div class="col-6 col-md-4 col-lg-3">
                <a href="/Seats/Detail/@seat.Id" class="text-decoration-none">
                    <div class="seat-card">
                        <h6 class="mb-2 fw-bold" style="color:#1e293b;">@seat.SeatNumber</h6>
                        <p class="mb-1 small text-secondary">@seat.Area · @seat.Floor</p>
                        <span class="status-badge status-@(seat.DisplayStatus switch {
                            "空闲" => "available",
                            "已预约" => "reserved",
                            "使用中" => "inuse",
                            _ => "maintenance"
                        })">@seat.DisplayStatus</span>
                    </div>
                </a>
            </div>
        }
    </div>
}
```

- [ ] **Step 3: Views/Seats/Detail.cshtml**

```cshtml
@model Seat
@{
    ViewData["Title"] = "座位详情";
}

<div class="card-info mb-4">
    <div class="d-flex justify-content-between align-items-start mb-3">
        <div>
            <h4 class="fw-bold mb-1">@Model.SeatNumber</h4>
            <p class="text-secondary mb-0">@Model.Area · @Model.Floor</p>
        </div>
        <span class="status-badge status-@(ViewBag.DisplayStatus switch {
            "空闲" => "available",
            "已预约" => "reserved",
            "使用中" => "inuse",
            _ => "maintenance"
        })" style="font-size:14px;padding:4px 14px;">@ViewBag.DisplayStatus</span>
    </div>

    @if (ViewBag.DisplayStatus == "空闲")
    {
        <a class="btn btn-primary btn-primary-custom mt-2" asp-controller="Reservation" asp-action="Create" asp-route-seatId="@Model.Id">
            预约此座位
        </a>
    }
    else
    {
        <p class="text-muted small mt-2 mb-0">当前不可预约</p>
    }
</div>

<h5 class="fw-bold mb-3">今日预约时段</h5>
@{
    var reservations = ViewBag.Reservations as IEnumerable<dynamic>;
}
@if (reservations == null || !reservations.Any())
{
    <div class="text-center py-4 text-muted">今日暂无预约记录</div>
}
else
{
    <ul class="list-group">
        @foreach (var r in reservations)
        {
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>
                    <strong>@r.UserName</strong>
                    <span class="text-secondary ms-2">@r.StartTime.ToString(@"hh\:mm") - @r.EndTime.ToString(@"hh\:mm")</span>
                </span>
                <span class="status-badge status-@(r.Status switch {
                    "待开始" => "pending",
                    "使用中" => "inuse",
                    "已完成" => "completed",
                    _ => "cancelled"
                })">@r.Status</span>
            </li>
        }
    </ul>
}

<div class="mt-3">
    <a href="javascript:history.back()" class="btn btn-outline-secondary">返回</a>
</div>
```

- [ ] **Step 4: 验证构建**

```bash
cd D:\AIWeb\LibrarySeatReservation
dotnet build
```

---
### Task 7: ReservationController — 提交预约 + 我的预约 + 取消

**文件：**
- 创建：`D:\AIWeb\LibrarySeatReservation\Controllers\ReservationController.cs`
- 创建：`D:\AIWeb\LibrarySeatReservation\Views\Reservation\Create.cshtml`
- 创建：`D:\AIWeb\LibrarySeatReservation\Views\Reservation\My.cshtml`

**依赖：** Task 2（数据模型）、Task 3（StatusHelper）、Task 4（Layout）

- [ ] **Step 1: ReservationController.cs**

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySeatReservation.Data;
using LibrarySeatReservation.Helpers;
using LibrarySeatReservation.Models;

namespace LibrarySeatReservation.Controllers;

public class ReservationController : Controller
{
    private readonly AppDbContext _db;

    public ReservationController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Create(int seatId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["ErrorMessage"] = "请先在首页选择体验账号";
            return RedirectToAction("Index", "Home");
        }

        var seat = _db.Seats.FirstOrDefault(s => s.Id == seatId);
        if (seat == null) return NotFound();

        // 检查座位当前是否空闲
        var today = DateTime.Today;
        var todayReservations = _db.Reservations
            .Where(r => r.SeatId == seatId && r.Date == today && r.Status == "Pending")
            .ToList();
        var displayStatus = StatusHelper.GetSeatDisplayStatus(seat.Status, todayReservations);
        if (displayStatus != "空闲")
        {
            TempData["ErrorMessage"] = "该座位当前不可预约";
            return RedirectToAction("Detail", "Seats", new { id = seatId });
        }

        ViewBag.Seat = seat;
        SetUserContext();
        return View();
    }

    [HttpPost]
    public IActionResult Create(int seatId, TimeSpan startTime, TimeSpan endTime)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["ErrorMessage"] = "请先选择体验账号";
            return RedirectToAction("Index", "Home");
        }

        // 时间范围校验
        if (endTime <= startTime)
        {
            ModelState.AddModelError("", "结束时间必须晚于开始时间");
        }
        if ((endTime - startTime).TotalMinutes < 30)
        {
            ModelState.AddModelError("", "最短预约时长为 30 分钟");
        }
        if ((endTime - startTime).TotalHours > 4)
        {
            ModelState.AddModelError("", "最长预约时长为 4 小时");
        }

        var seat = _db.Seats.FirstOrDefault(s => s.Id == seatId);
        if (seat == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Seat = seat;
            SetUserContext();
            return View();
        }

        // 冲突检查：该座位同一时间段是否有其他预约
        var today = DateTime.Today;
        var conflict = _db.Reservations.Any(r =>
            r.SeatId == seatId &&
            r.Date == today &&
            r.Status == "Pending" &&
            r.StartTime < endTime &&
            r.EndTime > startTime);

        if (conflict)
        {
            ViewBag.Seat = seat;
            ModelState.AddModelError("", "该时段已被预约，请选择其他时间");
            SetUserContext();
            return View();
        }

        var reservation = new Reservation
        {
            SeatId = seatId,
            StudentUserId = userId.Value,
            Date = today,
            StartTime = startTime,
            EndTime = endTime,
            Status = "Pending",
            CreatedAt = DateTime.Now
        };

        _db.Reservations.Add(reservation);
        _db.SaveChanges();

        TempData["SuccessMessage"] = "预约成功！";
        return RedirectToAction("My");
    }

    public IActionResult My()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["ErrorMessage"] = "请先选择体验账号";
            return RedirectToAction("Index", "Home");
        }

        var reservations = _db.Reservations
            .Include(r => r.Seat)
            .Where(r => r.StudentUserId == userId)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.StartTime)
            .ToList();

        // 为每条记录计算动态状态
        var displayList = reservations.Select(r => new
        {
            r.Id,
            SeatNumber = r.Seat.SeatNumber,
            Area = r.Seat.Area,
            r.Date,
            r.StartTime,
            r.EndTime,
            DisplayStatus = StatusHelper.GetReservationDisplayStatus(r.Status, r.StartTime, r.EndTime),
            CanCancel = r.Status == "Pending" && DateTime.Now.TimeOfDay < r.StartTime
        }).ToList();

        ViewBag.Reservations = displayList;
        SetUserContext();
        return View();
    }

    [HttpPost]
    public IActionResult Cancel(int id)
    {
        var reservation = _db.Reservations.Include(r => r.Seat).FirstOrDefault(r => r.Id == id);
        if (reservation == null) return NotFound();

        if (reservation.Status != "Pending")
        {
            TempData["ErrorMessage"] = "只能取消待开始的预约";
            return RedirectToAction("My");
        }

        // 只能取消自己的预约
        var userId = HttpContext.Session.GetInt32("UserId");
        if (reservation.StudentUserId != userId)
        {
            TempData["ErrorMessage"] = "只能取消自己的预约";
            return RedirectToAction("My");
        }

        reservation.Status = "Cancelled";
        _db.SaveChanges();

        TempData["SuccessMessage"] = "预约已取消";
        return RedirectToAction("My");
    }

    private void SetUserContext()
    {
        ViewBag.CurrentUser = HttpContext.Session.GetString("UserName") ?? "未选择";
        ViewBag.StudentUsers = _db.StudentUsers.OrderBy(u => u.Id).ToList();
    }
}
```

- [ ] **Step 2: Views/Reservation/Create.cshtml**

```cshtml
@{
    ViewData["Title"] = "提交预约";
    var seat = ViewBag.Seat as Seat;
}

<h4 class="fw-bold mb-4">提交预约</h4>

<div class="card-info mb-4">
    <p><strong>座位：</strong>@seat?.SeatNumber · @seat?.Area · @seat?.Floor</p>
</div>

<form method="post" asp-action="Create">
    <input type="hidden" name="seatId" value="@seat?.Id" />

    <div class="mb-3">
        <label class="form-label">开始时间</label>
        <select name="startTime" class="form-select" required>
            @for (int h = 8; h <= 21; h++)
            {
                for (int m = 0; m < 60; m += 30)
                {
                    var time = new TimeSpan(h, m, 0);
                    <option value="@time">@time.ToString(@"hh\:mm")</option>
                }
            }
        </select>
    </div>

    <div class="mb-3">
        <label class="form-label">结束时间</label>
        <select name="endTime" class="form-select" required>
            @for (int h = 8; h <= 22; h++)
            {
                for (int m = 0; m < 60; m += 30)
                {
                    var time = new TimeSpan(h, m, 0);
                    if (time > new TimeSpan(8, 0, 0))
                    {
                        <option value="@time">@time.ToString(@"hh\:mm")</option>
                    }
                }
            }
        </select>
    </div>

    <div class="text-muted small mb-3">⏱ 最短 30 分钟，最长 4 小时</div>

    @if (!ViewData.ModelState.IsValid)
    {
        <div class="alert alert-danger">
            @foreach (var error in ViewData.ModelState.Values.SelectMany(v => v.Errors))
            {
                <p class="mb-0">@error.ErrorMessage</p>
            }
        </div>
    }

    <div class="d-flex gap-2">
        <button type="submit" class="btn btn-primary btn-primary-custom">提交预约</button>
        <a href="javascript:history.back()" class="btn btn-outline-secondary">返回</a>
    </div>
</form>
```

- [ ] **Step 3: Views/Reservation/My.cshtml**

```cshtml
@{
    ViewData["Title"] = "我的预约";
}

<div class="d-flex justify-content-between align-items-center mb-3">
    <h4 class="fw-bold mb-0">我的预约</h4>
    <a class="btn btn-primary btn-primary-custom" asp-controller="Seats" asp-action="Index">继续预约</a>
</div>

@{
    var reservations = ViewBag.Reservations as IEnumerable<dynamic>;
}

@if (reservations == null || !reservations.Any())
{
    <div class="text-center py-5 text-muted">
        <p>你还没有预约记录</p>
        <a class="btn btn-primary btn-primary-custom mt-2" asp-controller="Seats" asp-action="Index">去预约</a>
    </div>
}
else
{
    <div class="row g-3">
        @foreach (var r in reservations)
        {
            <div class="col-12">
                <div class="card-info d-flex justify-content-between align-items-center">
                    <div>
                        <h6 class="fw-bold mb-1">@r.SeatNumber</h6>
                        <p class="mb-1 small text-secondary">
                            @r.Area · @r.Date.ToString("yyyy-MM-dd")
                            @r.StartTime.ToString(@"hh\:mm") - @r.EndTime.ToString(@"hh\:mm")
                        </p>
                        <span class="status-badge status-@(r.DisplayStatus switch {
                            "待开始" => "pending",
                            "使用中" => "inuse",
                            "已完成" => "completed",
                            _ => "cancelled"
                        })">@r.DisplayStatus</span>
                    </div>
                    <div>
                        @if (r.CanCancel)
                        {
                            <form method="post" asp-action="Cancel" asp-route-id="@r.Id"
                                  onsubmit="return confirm('确定取消此预约？')">
                                <button type="submit" class="btn btn-outline-danger btn-sm btn-danger-custom">取消预约</button>
                            </form>
                        }
                    </div>
                </div>
            </div>
        }
    </div>
}
```

- [ ] **Step 4: 验证构建**

```bash
cd D:\AIWeb\LibrarySeatReservation
dotnet build
```

---
### Task 8: AdminController — 登录/登出 + 管理端认证

**文件：**
- 创建：`D:\AIWeb\LibrarySeatReservation\Controllers\AdminController.cs`
- 创建：`D:\AIWeb\LibrarySeatReservation\Views\Admin\Login.cshtml`

**依赖：** Task 3（AdminAuthFilter）、Task 4（_AdminLayout）

- [ ] **Step 1: AdminController.cs（登录 + 登出部分）**

```csharp
using Microsoft.AspNetCore.Mvc;
using LibrarySeatReservation.Data;
using LibrarySeatReservation.Filters;

namespace LibrarySeatReservation.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    // ---- 登录 / 登出 ----

    [HttpGet]
    public IActionResult Login()
    {
        // 如果已登录，直接跳到管理页
        if (HttpContext.Session.GetString("AdminLoggedIn") == "true")
        {
            return RedirectToAction("Reservations");
        }
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        if (username == "admin" && password == "admin123")
        {
            HttpContext.Session.SetString("AdminLoggedIn", "true");
            HttpContext.Session.SetString("AdminName", "管理员");
            return RedirectToAction("Reservations");
        }

        ViewBag.Error = "用户名或密码错误";
        return View();
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("AdminLoggedIn");
        HttpContext.Session.Remove("AdminName");
        return RedirectToAction("Login");
    }
}
```

- [ ] **Step 2: Views/Admin/Login.cshtml**

```cshtml
@{
    Layout = "_AdminLayout";
    ViewData["Title"] = "管理员登录";
}

<div class="row justify-content-center mt-5">
    <div class="col-md-5 col-lg-4">
        <div class="card-info">
            <h4 class="fw-bold text-center mb-4">管理员登录</h4>

            @if (ViewBag.Error != null)
            {
                <div class="alert alert-danger">@ViewBag.Error</div>
            }

            <form method="post">
                <div class="mb-3">
                    <label class="form-label">用户名</label>
                    <input type="text" name="username" class="form-control" value="admin" required />
                </div>
                <div class="mb-3">
                    <label class="form-label">密码</label>
                    <input type="password" name="password" class="form-control" value="admin123" required />
                </div>
                <button type="submit" class="btn btn-primary w-100 btn-primary-custom">登录</button>
            </form>

            <div class="text-center mt-3">
                <a href="/" class="text-decoration-none small">← 返回首页</a>
            </div>
        </div>
    </div>
</div>
```

- [ ] **Step 3: 验证构建**

```bash
cd D:\AIWeb\LibrarySeatReservation
dotnet build
```

---
### Task 9: AdminController — 管理预约 + 管理座位 + 统计页

**文件：**
- 追加到：`D:\AIWeb\LibrarySeatReservation\Controllers\AdminController.cs`（追加管理端操作 Action）
- 创建：`D:\AIWeb\LibrarySeatReservation\Views\Admin\Reservations.cshtml`
- 创建：`D:\AIWeb\LibrarySeatReservation\Views\Admin\Seats.cshtml`
- 创建：`D:\AIWeb\LibrarySeatReservation\Views\Admin\Stats.cshtml`

**依赖：** Task 8（AdminController 基础结构）、Task 3（StatusHelper）

- [ ] **Step 1: 在 AdminController 追加管理端 Action**

在 AdminController 的 Login/Logout 方法后面追加以下内容：

```csharp
    // ---- 预约管理 ----

    [AdminAuth]
    public IActionResult Reservations()
    {
        var reservations = _db.Reservations
            .Include(r => r.Seat)
            .Include(r => r.StudentUser)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.StartTime)
            .ToList();

        var displayList = reservations.Select(r => new
        {
            r.Id,
            UserName = r.StudentUser.Name,
            SeatNumber = r.Seat.SeatNumber,
            r.Seat.Area,
            r.Date,
            r.StartTime,
            r.EndTime,
            Status = StatusHelper.GetReservationDisplayStatus(r.Status, r.StartTime, r.EndTime)
        }).ToList();

        ViewBag.Reservations = displayList;
        SetAdminViewBag();
        return View();
    }

    // ---- 座位管理 ----

    [AdminAuth]
    public IActionResult Seats()
    {
        var seats = _db.Seats.OrderBy(s => s.SeatNumber).ToList();
        return View(seats);
    }

    [AdminAuth]
    [HttpPost]
    public IActionResult CreateSeat(string seatNumber, string area, string floor)
    {
        if (string.IsNullOrWhiteSpace(seatNumber) || string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(floor))
        {
            TempData["ErrorMessage"] = "请填写所有字段";
            return RedirectToAction("Seats");
        }

        if (_db.Seats.Any(s => s.SeatNumber == seatNumber))
        {
            TempData["ErrorMessage"] = "该座位编号已存在";
            return RedirectToAction("Seats");
        }

        var seat = new Models.Seat
        {
            SeatNumber = seatNumber,
            Area = area,
            Floor = floor,
            Status = "Available"
        };

        _db.Seats.Add(seat);
        _db.SaveChanges();

        TempData["SuccessMessage"] = $"座位 {seatNumber} 已添加";
        return RedirectToAction("Seats");
    }

    // ---- 统计 ----

    [AdminAuth]
    public IActionResult Stats()
    {
        var today = DateTime.Today;

        var totalSeats = _db.Seats.Count();
        var todayReservations = _db.Reservations.Count(r => r.Date == today);
        var todayReservedSeats = _db.Reservations
            .Where(r => r.Date == today && r.Status == "Pending")
            .Select(r => r.SeatId)
            .Distinct()
            .Count();

        var usageRate = totalSeats > 0 ? (double)todayReservedSeats / totalSeats * 100 : 0;

        // 各区域分布
        var areaDistribution = _db.Seats
            .GroupBy(s => s.Area)
            .Select(g => new { Area = g.Key, Count = g.Count() })
            .ToList();

        ViewBag.TotalSeats = totalSeats;
        ViewBag.TodayReservations = todayReservations;
        ViewBag.UsageRate = usageRate;
        ViewBag.AreaDistribution = areaDistribution;

        SetAdminViewBag();
        return View();
    }

    private void SetAdminViewBag()
    {
        ViewBag.AdminName = HttpContext.Session.GetString("AdminName") ?? "管理员";
    }
```

注意：确保 `AdminController` 的类定义已经是 `public class AdminController : Controller`。

- [ ] **Step 2: Views/Admin/Reservations.cshtml**

```cshtml
@{
    Layout = "_AdminLayout";
    ViewData["Title"] = "预约管理";
}

<h4 class="fw-bold mb-3">全部预约记录</h4>

@{
    var reservations = ViewBag.Reservations as IEnumerable<dynamic>;
}

@if (reservations == null || !reservations.Any())
{
    <div class="text-center py-5 text-muted">暂无预约记录</div>
}
else
{
    <div class="table-responsive">
        <table class="table table-admin table-hover">
            <thead>
                <tr>
                    <th>预约人</th>
                    <th>座位</th>
                    <th>区域</th>
                    <th>日期</th>
                    <th>时段</th>
                    <th>状态</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var r in reservations)
                {
                    <tr>
                        <td>@r.UserName</td>
                        <td>@r.SeatNumber</td>
                        <td>@r.Area</td>
                        <td>@r.Date.ToString("yyyy-MM-dd")</td>
                        <td>@r.StartTime.ToString(@"hh\:mm") - @r.EndTime.ToString(@"hh\:mm")</td>
                        <td>
                            <span class="status-badge status-@(r.Status switch {
                                "待开始" => "pending",
                                "使用中" => "inuse",
                                "已完成" => "completed",
                                _ => "cancelled"
                            })">@r.Status</span>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}
```

- [ ] **Step 3: Views/Admin/Seats.cshtml**

```cshtml
@model List<Seat>
@{
    Layout = "_AdminLayout";
    ViewData["Title"] = "座位管理";
}

<h4 class="fw-bold mb-3">座位管理</h4>

<!-- 添加座位 -->
<div class="card-info mb-4">
    <h6 class="fw-bold mb-3">添加新座位</h6>
    <form method="post" asp-action="CreateSeat" class="row g-2">
        <div class="col-md-3">
            <input type="text" name="seatNumber" class="form-control" placeholder="座位编号（如 D-01）" required />
        </div>
        <div class="col-md-3">
            <input type="text" name="area" class="form-control" placeholder="区域（如四楼讨论区）" required />
        </div>
        <div class="col-md-3">
            <input type="text" name="floor" class="form-control" placeholder="楼层（如 4F）" required />
        </div>
        <div class="col-md-3">
            <button type="submit" class="btn btn-primary w-100">添加</button>
        </div>
    </form>
</div>

<!-- 现有座位 -->
@if (!Model.Any())
{
    <div class="text-center py-4 text-muted">暂无座位，请添加</div>
}
else
{
    <div class="table-responsive">
        <table class="table table-admin table-hover">
            <thead>
                <tr>
                    <th>编号</th>
                    <th>区域</th>
                    <th>楼层</th>
                    <th>状态</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var seat in Model)
                {
                    <tr>
                        <td>@seat.SeatNumber</td>
                        <td>@seat.Area</td>
                        <td>@seat.Floor</td>
                        <td>
                            <span class="status-badge status-@(seat.Status == "Available" ? "available" : "maintenance")">
                                @(seat.Status == "Available" ? "空闲" : "维护中")
                            </span>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}
```

- [ ] **Step 4: Views/Admin/Stats.cshtml**

```cshtml
@{
    Layout = "_AdminLayout";
    ViewData["Title"] = "统计";
}

<h4 class="fw-bold mb-4">数据统计</h4>

<div class="row g-3 mb-4">
    <div class="col-md-4">
        <div class="stats-card">
            <div class="stats-number">@ViewBag.TotalSeats</div>
            <div class="stats-label">座位总数</div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="stats-card">
            <div class="stats-number">@ViewBag.TodayReservations</div>
            <div class="stats-label">今日预约数</div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="stats-card">
            <div class="stats-number">@Math.Round(ViewBag.UsageRate, 1)%</div>
            <div class="stats-label">座位使用率</div>
        </div>
    </div>
</div>

<h5 class="fw-bold mb-3">各区域座位分布</h5>
<table class="table table-admin">
    <thead>
        <tr>
            <th>区域</th>
            <th>座位数</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in ViewBag.AreaDistribution)
        {
            <tr>
                <td>@item.Area</td>
                <td>@item.Count</td>
            </tr>
        }
    </tbody>
</table>
```

- [ ] **Step 5: 验证完整构建**

```bash
cd D:\AIWeb\LibrarySeatReservation
dotnet build
```

预期：Build succeeded，0 warnings

---
### Task 10: 运行 + 最终验证

**文件：** 无新建，运行和确认

- [ ] **Step 1: 清理旧数据库并重新创建**

```bash
cd D:\AIWeb\LibrarySeatReservation
# 删除旧数据库（如果有）
dotnet run &
Start-Sleep -Seconds 8
```

实际手动测试步骤：
1. 访问 `http://localhost:5000`（或终端显示的端口）→ 看到用户首页
2. 顶部导航栏右侧下拉框选择"学生A"
3. 点击"座位列表"→ 看到 12 个座位卡片
4. 点击一个空闲座位 → 看到座位详情
5. 点击"预约此座位"→ 选择时间 → 提交
6. 跳转到"我的预约"→ 看到新预约记录
7. 访问 `/Admin/Login` → 输入 admin / admin123
8. 看到预约管理页（全部预约记录）
9. 点击导航"座位管理"→ 看到座位列表 + 可添加座位
10. 点击导航"统计"→ 看到座位统计数据

- [ ] **Step 2: 验证构建最终通过**

```bash
cd D:\AIWeb\LibrarySeatReservation
dotnet build
```

---
## 自审清单

**1. 需求覆盖：**
- F1（体验账号切换）→ Task 5 HomeController.SwitchUser + Index 页面
- F2（浏览座位）→ Task 6 SeatsController.Index + List View
- F3（查看座位详情）→ Task 6 SeatsController.Detail + Detail View
- F4（选择时段提交预约）→ Task 7 ReservationController.Create
- F5（查看我的预约）→ Task 7 ReservationController.My
- F6（取消预约）→ Task 7 ReservationController.Cancel
- F7（管理员登录）→ Task 8 AdminController.Login
- F8（管理预约/座位/统计）→ Task 9 AdminController.Reservations/Seats/Stats
- 状态动态计算 → Task 3 StatusHelper
- Session 认证 → Task 3 AdminAuthFilter + Task 8

**2. 占位符检查：** 无"TBD/TODO/待实现"等占位符，所有代码完整

**3. 类型一致性：**
- `StatusHelper.GetSeatDisplayStatus(string, List<Reservation>)` → Task 3 定义，Task 6+9 调用 → 一致
- `StatusHelper.GetReservationDisplayStatus(string, TimeSpan, TimeSpan)` → Task 3 定义，Task 7+9 调用 → 一致
- `SeatDisplay` model → Task 2 定义，Task 6 使用 → 一致
- `AdminAuth` → Task 3 定义，Task 9 使用 → 一致
