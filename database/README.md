# 数据库初始化说明 — 图书馆座位预约系统

---

## 1. 数据库选型

| 项目 | 内容 |
|------|------|
| 数据库系统 | SQL Server LocalDB（Visual Studio 内置） |
| ORM | Entity Framework Core 9.x（Code First） |
| 连接字符串 | `appsettings.json` 中 `ConnectionStrings:DefaultConnection` |
| 目标数据库名 | `LibrarySeatReservation` |

> **降级方案**：若 LocalDB 不可用，可将 `Program.cs` 中 `UseSqlServer` 替换为 `UseSqlite`，
> 连接字符串改为 `Data Source=LibrarySeat.db`，其余代码无需修改。

---

## 2. 首次建库建表

### 前提

- .NET 9 SDK 已安装
- SQL Server LocalDB 可用（`SqlLocalDB info` 确认）

### 执行命令

```bash
dotnet restore
dotnet ef database update --project src/LibrarySeatReservation.Web
```

该命令会：
1. 读取 `Migrations/` 目录中的迁移文件
2. 在 LocalDB 中创建 `LibrarySeatReservation` 数据库
3. 执行所有挂起的迁移，创建 3 张表

### 自动迁移（运行时）

`Program.cs` 中以下代码在每次启动时自动执行挂起的迁移：

```csharp
db.Database.Migrate();  // 自动执行挂起的迁移（首次建表）
DbInitializer.Initialize(db);  // 写入种子数据
```

这意味着**首次启动时无需手动执行 `dotnet ef database update`**，应用会自动完成建库建表。

---

## 3. 数据表结构

| 表名 | 说明 | 主要字段 |
|------|------|---------|
| `StudentUsers` | 学生体验账号 | Id, Name |
| `Seats` | 座位信息 | Id, SeatNumber, Area, Floor, Status |
| `Reservations` | 预约记录 | Id, SeatId, StudentUserId, Date, StartTime, EndTime, Status, CreatedAt |

### 索引

| 表 | 索引名 | 列 | 用途 |
|----|--------|----|------|
| Seats | `IX_Seats_Area_Floor` | Area, Floor | 按区域/楼层筛选 |
| Reservations | `IX_Reservations_SeatId_Date` | SeatId, Date | 预约冲突检查 |
| Reservations | `IX_Reservations_StudentUserId` | StudentUserId | 我的预约查询 |
| Reservations | `IX_Reservations_Date_StartTime` | Date, StartTime | 管理端按时间倒序 |

### Status 字段取值

| 表 | 持久化值 | 说明 |
|----|---------|------|
| Seats.Status | `Available` / `Maintenance` | 数据库真实状态 |
| Reservations.Status | `Pending` / `Cancelled` | 数据库真实状态 |

> 前端展示的"动态状态"（如"使用中""待开始""已完成"）由 `StatusHelper` 根据 `Date`+`StartTime`+`EndTime` 在当前时间下的对比计算得出，不持久化。

---

## 4. 种子数据初始化

### 初始化时机

`Program.cs` → `db.Database.Migrate()` → `DbInitializer.Initialize(db)`

`DbInitializer.Initialize()` 检测到 `StudentUsers` 表非空时自动跳过，因此**多次启动不会重复写入**。

### 种子数据内容

#### 体验学生（5 个）

| Name |
|------|
| 学生A |
| 学生B |
| 学生C |
| 学生D |
| 学生E |

#### 座位（12 个，3 区域 × 4 座位）

| 座位编号 | 区域 | 楼层 | 初始状态 |
|---------|------|------|---------|
| A-01 ~ A-04 | 一楼大厅 | 1F | Available |
| B-01 ~ B-04 | 二楼阅览室 | 2F | Available |
| C-01 ~ C-04 | 三楼自习区 | 3F | Available |

#### 预约记录

种子数据**不写入**预约记录。预约数据在用户正常使用系统时产生。

---

## 5. 演示账号

| 角色 | 账号 | 说明 |
|------|------|------|
| 学生体验账号 | 学生A ~ 学生E | 首页下拉框切换，无需密码 |
| 管理员 | admin / admin123 | 访问 `/Admin/Login` 登录 |

管理员账号由 `AdminController.Login` 中的硬编码校验，不存储在数据库中。

---

## 6. Code First 迁移说明

### 迁移目录

`src/LibrarySeatReservation.Web/Migrations/`

### 已创建迁移

首次通过 `dotnet ef migrations add InitialCreate` 创建。

### 如需修改模型

```bash
dotnet ef migrations add MigrationName --project src/LibrarySeatReservation.Web
dotnet ef database update --project src/LibrarySeatReservation.Web
```

### 迁移文件内容

包含 `AppDbContext.OnModelCreating()` 中定义的全部表结构：
- 3 张表的主键、字段约束、最大长度
- 4 个索引
- 2 个外键关系（Cascade 删除）
- 默认值（Seats.Status = "Available", Reservations.Status = "Pending", CreatedAt = GETDATE()）
