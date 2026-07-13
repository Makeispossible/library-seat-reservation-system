using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LibrarySeatReservation.Web.Data;
using LibrarySeatReservation.Web.Models;

namespace LibrarySeatReservation.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<HomeController> _logger;

    public HomeController(AppDbContext db, ILogger<HomeController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 用户首页 — 显示学生列表供切换账号
    /// </summary>
    public IActionResult Index()
    {
        ViewBag.StudentUsers = _db.StudentUsers.OrderBy(u => u.Id).ToList();
        ViewBag.CurrentUser = HttpContext.Session.GetString("UserName") ?? "未选择";
        return View();
    }

    /// <summary>
    /// 切换体验账号 — POST 接收 userId，写入 Session
    /// </summary>
    [HttpPost]
    public IActionResult SwitchUser(int userId)
    {
        var user = _db.StudentUsers.FirstOrDefault(u => u.Id == userId);
        if (user != null)
        {
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
        }

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
