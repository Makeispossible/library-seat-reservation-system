# 开发准备与 Sprint 0 — 图书馆座位预约系统

> 本文档基于 `docs/07-系统设计说明.md`、`docs/08-数据库设计.md`、`docs/09-关键链路详细设计.md` 编写。
> 属于"开发准备助理 + Sprint 规划助理"阶段输出。

---

## 1. 仓库结构

```
library-seat-reservation-system/      ← GitHub 仓库
├── LibrarySeatReservation/           ← ASP.NET Core MVC 项目目录
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── DbInitializer.cs
│   ├── Helpers/
│   ├── Filters/
│   ├── Views/
│   │   ├── Home/
│   │   ├── Seats/
│   │   ├── Reservation/
│   │   ├── Admin/
│   │   └── Shared/
│   ├── wwwroot/
│   ├── Program.cs
│   ├── appsettings.json
│   └── LibrarySeatReservation.csproj
├── docs/                             ← 项目文档目录（本仓库的一部分）
│   ├── 01-项目立项单.md
│   ├── 02-需求分析与MVP确认.md
│   ├── 03-PRD-Lite.md
│   ├── 04-页面树与业务流程.md
│   ├── 05-页面卡与UI规范.md
│   ├── 07-系统设计说明.md
│   ├── 08-数据库设计.md
│   ├── 09-关键链路详细设计.md
│   ├── 09-关键链路详细设计-审计.md
│   ├── 10-开发准备与Sprint0.md      ← 本文件
│   ├── 项目任务板与迭代记录.md        ← 任务跟踪
│   └── superpowers/plans/
├── prototype/                        ← 静态原型
├── README.md
└── opencode.json
```

**说明：** 仓库根目录即项目根目录。`.sln` 解决方案文件直接放在 `LibrarySeatReservation/` 外层（即仓库根目录）或 `LibrarySeatReservation/` 目录内均可。本期约定：`.sln` 文件放在仓库根目录，Web 项目在 `LibrarySeatReservation/` 子目录。

---

## 2. 分支策略

| 分支 | 用途 | 保护 |
|------|------|------|
| `main` | 生产就绪代码，每次 Sprint 结束合并 | ✅ 受保护，需 PR |
| `dev` | 日常开发集成分支 | ❌ 不保护 |
| `feat/<name>` | 单个功能开发分支，从 `dev` 切出 | ❌ 短期存在 |

**工作流：**

```
feat/home-page → PR → dev → 验证 → PR → main
feat/seats     → PR → dev ↗
feat/reserve   → PR → dev ↗
feat/admin     → PR → dev ↗
```

**规则：**
- 每个任务卡对应一个 `feat/` 分支
- 完成任务后 PR 到 `dev`，在 `dev` 上做集成验证
- Sprint 结束日 PR `dev → main`

---

## 3. 提交规范

### 提交信息格式

```
<类型>(<范围>): <简短描述>

<可选详细说明>
```

### 类型

| 类型 | 何时使用 |
|------|----------|
| `feat` | 新功能 |
| `fix` | 修复 Bug |
| `docs` | 文档变更 |
| `refactor` | 重构 |
| `chore` | 构建/工具/配置变更 |
| `test` | 测试 |

### 示例

```
feat(home): 实现体验账号切换功能
docs(arch): 更新系统设计说明中的路由表
fix(reserve): 修复结束时间等于开始时间仍可通过校验的问题
chore(project): 创建 .sln 和项目骨架
```

### 原则

- 一个 commit 对应一个原子变更
- 不要混入无关修改
- 提交信息用简体中文描述内容

---

## 4. Sprint 0 目标

**Sprint 0 是开发准备阶段，不写业务代码。目标是搭好开发运行环境。**

### Sprint 0 任务一览

| # | 任务 | 验收标准 |
|---|------|----------|
| T0-01 | 创建 .sln 解决方案文件 | `dotnet build` 通过 |
| T0-02 | 创建 ASP.NET Core MVC 项目 | `dotnet run` 启动成功，浏览器可访问 |
| T0-03 | 安装 EF Core + SQL Server NuGet 包 | `dotnet build` 无缺失引用错误 |
| T0-04 | 配置 appsettings.json 连接字符串 | 连接字符串指向 LocalDB |
| T0-05 | 创建 Models（StudentUser, Seat, Reservation, SeatDisplay, ErrorViewModel） | 编译通过 |
| T0-06 | 创建 AppDbContext + DbInitializer（含种子数据 5 学生 + 12 座位） | `EnsureCreated` 可建库 |
| T0-07 | 配置 Session 服务 + 中间件 | 应用可写读 Session |
| T0-08 | 首次 EF Core 迁移 + 建库建表 | 数据库中生成了 3 张表 |
| T0-09 | 首次 `dotnet run` 验证全流程 | 启动 → 建库 → 种子数据 → 浏览器页面可达 |
| T0-10 | 本地仓库初始化 + 首次提交 + 推送到远端 | GitHub 仓库可见首次提交 |

---

## 5. Sprint 1-4 主 Sprint 粗计划

> 每个主 Sprint 为**多轮推进**，即 Sprint 1 内部可包含多轮（Sprint 1 轮次 1、轮次 2……），每轮完成后在任务板中更新状态。段落后标注的"阶段最低完成线"是进入下一阶段的硬性门槛。

### Sprint 1：用户端首页 + 座位浏览

| 项目 | 内容 |
|------|------|
| 目标 | 完成用户端 3 页：首页、座位列表、座位详情 |
| 涉及功能 | F1 体验账号切换、F2 座位列表+筛选、F3 座位详情 |
| 涉及文档 | HomeController, SeatsController, StatusHelper, Shared Layout |
| 阶段最低完成线 | 首页可显示、体验账号可切换并持久化、座位列表可展示 12 个座位带筛选 |
| 编码顺序参考 | 09-关键链路详细设计 第 5.1 节步骤 1~7 |
| 允许并行 | Home 和 Seats 可并行开发 |
| 当前轮次 | 第 1 轮 |
| 任务统计 | 待定 |
| 本轮新增完成卡片 | 待定 |
| 当前完成率 | 0% |
| 是否允许进入下一阶段 | 达到最低完成线后方可进入 Sprint 2 |

**Sprint 1 关键交付物：**
- 项目可 `dotnet run` 启动
- 首页显示学生列表下拉框，切换后顶部显示"当前用户：学生X"
- 座位列表显示 12 个座位卡片，按区域/楼层筛选正常
- 座位详情页显示座位信息 + 今日预约时段
- `StatusHelper` 正确计算座位动态状态（空闲/已预约/使用中/维护中）

### Sprint 2：预约 + 取消

| 项目 | 内容 |
|------|------|
| 目标 | 完成用户端剩余 2 页：预约提交、我的预约，含取消功能 |
| 涉及功能 | F4 提交预约、F5 我的预约、F6 取消预约 |
| 涉及文档 | ReservationController, Create.cshtml, My.cshtml |
| 阶段最低完成线 | 可完成完整预约流程（首页 → 选座 → 提交 → 查看）和取消流程 |
| 编码顺序参考 | 09-关键链路详细设计 第 5.1 节步骤 8 |
| 当前轮次 | 第 1 轮 |
| 任务统计 | 待定 |
| 本轮新增完成卡片 | 待定 |
| 当前完成率 | 0% |
| 是否允许进入下一阶段 | 达到最低完成线后方可进入 Sprint 3 |

**Sprint 2 关键交付物：**
- 预约表单：选时 → 校验（30分钟~4小时）→ 冲突检查 → 写入数据库
- 预约成功跳转"我的预约"页
- 我的预约页按时间倒序展示，动态状态正确
- 取消预约：弹窗确认 → 状态变为已取消
- 链路 C（多账号对比）自动验证通过

### Sprint 3：管理端功能

| 项目 | 内容 |
|------|------|
| 目标 | 完成管理端 4 页：登录、预约管理、座位管理、统计 |
| 涉及功能 | F7 管理员登录、F8 座位管理、F9 预约记录查看、统计 |
| 涉及文档 | AdminController + 全部 4 个视图 + AdminAuthFilter |
| 阶段最低完成线 | 管理员可登录、可查看预约记录、可添加座位、统计数字正确 |
| 编码顺序参考 | 09-关键链路详细设计 第 5.1 节步骤 9（可与 Sprint 2 部分并行） |
| 当前轮次 | 第 1 轮 |
| 任务统计 | 待定 |
| 本轮新增完成卡片 | 待定 |
| 当前完成率 | 0% |
| 是否允许进入下一阶段 | 达到最低完成线后方可进入 Sprint 4 |

**Sprint 3 关键交付物：**
- AdminAuthFilter 守卫管理端页面
- 登录页校验 admin/admin123
- 预约管理页展示全部预约（关联 Seat + StudentUser）
- 座位管理页展示所有座位 + 添加表单
- 统计页显示 4 个指标（座位总数/今日预约数/使用率/区域分布）
- `/Admin/Logout` 登出功能

### Sprint 4：集成 + 审计 + 交付

| 项目 | 内容 |
|------|------|
| 目标 | 全链路集成测试 + 代码一致性总审计 + 修正 + 交付 |
| 涉及文档 | Program.cs 最终组装、全功能验证、prd 验收标准逐条检查 |
| 编码顺序参考 | 09-关键链路详细设计 第 5.1 节步骤 10 |
| 阶段最低完成线 | 所有 PRD 验收标准通过，项目可运行演示 |
| 当前轮次 | 第 1 轮 |
| 任务统计 | 待定 |
| 本轮新增完成卡片 | 待定 |
| 当前完成率 | 0% |
| 是否允许进入下一阶段 | 此阶段为最终阶段 |

**Sprint 4 关键交付物：**
- Program.cs 最终组装（Session、EF Core、路由）
- 开发前一致性总审计（检查所有文档 → 代码一致性）
- PRD 验收标准 15 条逐条通过
- 最终提交 + 推送远端

---

## 6. 里程碑节点

> 共 4 个里程碑，对应 4 个 Sprint 的完成节点。

| 里程碑 | 关联 Sprint | 预计达成 | 验收标准 |
|--------|-----------|---------|----------|
| **M1：项目可运行** | Sprint 0 | Sprint 0 结束日 | `dotnet run` 启动，3 张表建好，种子数据就位 |
| **M2：用户端可用** | Sprint 1 + 2 | Sprint 2 结束日 | 链路 A（预约）和链路 B（取消）完整跑通 |
| **M3：管理端可用** | Sprint 3 | Sprint 3 结束日 | 链路 D（管理登录+管理座位）和链路 E（统计）完整跑通 |
| **M4：交付就绪** | Sprint 4 | Sprint 4 结束日 | PRD 15 条验收标准全部通过，可演示 |

---

## 7. 默认补足项 / 当前假设

### 前序文档已覆盖项

以下内容在前序文档（07/08/09）中已有明确决策，本次不再重复，直接沿用：

- 技术栈 ASP.NET Core MVC .NET 9 + SQL Server LocalDB
- 不使用 Service 层 / Repository 模式 / Identity Framework / Areas / AutoMapper
- 3 表结构 + 4 个索引 + 5 学生 + 12 座位种子数据
- 5 条链路定义（A/B/C/D/E）
- 动态状态计算方案

### 本次补充项

| 补足项 | 本次决策 | 依据 |
|--------|---------|------|
| `.sln` 位置 | 仓库根目录 | 便于 VS/VS Code 直接打开完整解决方案 |
| 项目名空间 | `LibrarySeatReservation` | 与项目名保持一致 |
| NuGet 包版本 | EF Core 9.x 最新稳定版 | 匹配 .NET 9 |
| 迁移方式 | `EnsureCreated()` + `DbInitializer` | 简单项目，不使用迁移（Sprint 0 用 EnsureCreated，后续不改模型） |
| 首次提交范围 | 所有现有文档 + 3 个新文件 | 不含业务代码（业务代码在 Sprint 1~3 按 feat/ 分支提交） |

### 当前假设

| 假设 | 影响 |
|------|------|
| 模型在 Sprint 0 确定后不再变更 | 不需要使用 EF Core 迁移（migration），用 `EnsureCreated` 即可 |
| 项目只在本机开发 | 无 CI/CD、无 Docker、无多环境配置 |
| 只在开发环境运行 | 不配置生产级异常处理、日志级别 |
| 学生不修改种子数据 | 数据库重置只需删除 `.mdf` 文件重新运行 |
