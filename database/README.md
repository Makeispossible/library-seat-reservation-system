# 数据库初始化说明 — 图书馆座位预约系统

## 方案：Entity Framework Core Code First + Migrations

本项目采用 **EF Core Code First** 方式管理数据库：

1. **模型类**（`src/LibrarySeatReservation.Web/Models/`）定义数据结构
2. **AppDbContext**（`src/LibrarySeatReservation.Web/Data/AppDbContext.cs`）配置表映射、索引和外键
3. **Migration 脚本**（`src/LibrarySeatReservation.Web/Migrations/`）由 EF Core CLI 生成，描述建表操作
4. **DbInitializer**（`src/LibrarySeatReservation.Web/Data/DbInitializer.cs`）在首次运行时写入种子数据

---

## 数据表结构

### StudentUsers — 体验学生账号

| 字段 | 类型 | 约束 |
|------|------|------|
| Id | int | 主键，自增 |
| Name | nvarchar(50) | 非空 |

### Seats — 座位

| 字段 | 类型 | 约束 |
|------|------|------|
| Id | int | 主键，自增 |
| SeatNumber | nvarchar(20) | 非空 |
| Area | nvarchar(50) | 非空 |
| Floor | nvarchar(10) | 非空 |
| Status | nvarchar(20) | 非空，默认值 `Available` |

索引：`IX_Seats_Area_Floor`（Area + Floor 复合索引，用于筛选查询）

### Reservations — 预约记录

| 字段 | 类型 | 约束 |
|------|------|------|
| Id | int | 主键，自增 |
| SeatId | int | 外键 → Seats.Id，CASCADE 删除 |
| StudentUserId | int | 外键 → StudentUsers.Id，CASCADE 删除 |
| Date | date | 非空 |
| StartTime | time(7) | 非空 |
| EndTime | time(7) | 非空 |
| Status | nvarchar(20) | 非空，默认值 `Pending` |
| CreatedAt | datetime | 默认值 `GETDATE()` |

索引：
- `IX_Reservations_SeatId_Date`（SeatId + Date 复合索引，用于冲突检查）
- `IX_Reservations_StudentUserId`（StudentUserId 索引，用于"我的预约"查询）
- `IX_Reservations_Date_StartTime`（Date + StartTime 复合索引，用于管理端排序）

### 持久化状态枚举

| 实体 | 持久化值 | 含义 |
|------|---------|------|
| Seat.Status | `Available` | 可用 |
| Seat.Status | `Maintenance` | 维护中 |
| Reservation.Status | `Pending` | 待开始（预约已提交，尚未开始或已结束） |
| Reservation.Status | `Cancelled` | 已取消 |

> 展示给用户的中文状态（如"空闲""使用中""已预约"）由 `StatusHelper` 根据持久化状态 + 当前时间动态计算，不持久化存储。

---

## 首次建库建表

### 方法一：手动执行迁移（推荐首次）

```bash
dotnet ef database update --project src/LibrarySeatReservation.Web
```

### 方法二：自动迁移（启动时执行）

Program.cs 中的以下代码会在应用启动时自动执行挂起的迁移：

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();   // 自动执行挂起的迁移
    DbInitializer.Initialize(db);  // 写入种子数据
}
```

因此直接运行 `dotnet run` 也会自动建表。

### 前提条件

- SQL Server LocalDB 已安装并可用（验证：`SqlLocalDB info`）
- 连接字符串在 `appsettings.json` 中配置：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=LibrarySeatReservation;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

---

## 种子数据

种子数据在 `DbInitializer.Initialize()` 中定义。首次启动时自动写入，检测到 StudentUsers 表已有数据则跳过。

### 5 个体验学生

| ID | 姓名 |
|----|------|
| 1 | 学生A |
| 2 | 学生B |
| 3 | 学生C |
| 4 | 学生D |
| 5 | 学生E |

### 12 个座位

| 座位编号 | 区域 | 楼层 | 初始状态 |
|---------|------|------|---------|
| A-01 | 一楼大厅 | 1F | Available |
| A-02 | 一楼大厅 | 1F | Available |
| A-03 | 一楼大厅 | 1F | Available |
| A-04 | 一楼大厅 | 1F | Available |
| B-01 | 二楼阅览室 | 2F | Available |
| B-02 | 二楼阅览室 | 2F | Available |
| B-03 | 二楼阅览室 | 2F | Available |
| B-04 | 二楼阅览室 | 2F | Available |
| C-01 | 三楼自习区 | 3F | Available |
| C-02 | 三楼自习区 | 3F | Available |
| C-03 | 三楼自习区 | 3F | Available |
| C-04 | 三楼自习区 | 3F | Available |

---

## 默认账号

| 角色 | 账号 | 密码 | 使用方式 |
|------|------|------|---------|
| 学生体验账号 | 学生A ~ 学生E | 无需密码 | 首页下拉框切换，Session 保持 |
| 管理员 | admin | admin123 | 访问 `/Admin/Login` 登录 |

---

## 降级方案：SQLite

如果目标环境没有 SQL Server LocalDB，可按以下步骤切换到 SQLite：

1. 安装 SQLite 包：
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.Sqlite
   ```

2. 修改 Program.cs：
   ```csharp
   // 替换：
   options.UseSqlServer(...)
   // 为：
   options.UseSqlite("Data Source=LibrarySeat.db")
   ```

3. 重新生成 Migration：
   ```bash
   dotnet ef migrations remove
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

---

## 数据库重置

如需清空数据并重新初始化：

```bash
# 删除数据库
dotnet ef database drop --project src/LibrarySeatReservation.Web

# 重新建库
dotnet ef database update --project src/LibrarySeatReservation.Web

# 或直接运行应用（自动建库 + 种子数据）
dotnet run --project src/LibrarySeatReservation.Web
```
