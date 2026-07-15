using Microsoft.EntityFrameworkCore;
using LibrarySeatReservation.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// === 服务注册 ===

// EF Core — SQL Server LocalDB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session（用于体验账号切换 + 管理端认证）
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// === 中间件管道 ===

// 错误页面（开发环境显示详细错误，生产环境用自定义页面）
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error"); // 开发环境也使用自定义错误页，可通过 ASpNETCORE_ENVIRONMENT 切换
}

// 状态码页面（404/403 等）
app.UseStatusCodePagesWithReExecute("/Home/HandleStatusCode", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// === 数据库初始化（种子数据） ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // 自动执行挂起的迁移（首次建表）
    DbInitializer.Initialize(db); // 写入种子数据
}

app.Run();
