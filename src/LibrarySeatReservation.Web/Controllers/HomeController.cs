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
    /// 首页 — 根据登录状态显示不同内容
    /// </summary>
    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        ViewBag.IsLoggedIn = userId.HasValue;
        ViewBag.CurrentUser = HttpContext.Session.GetString("UserName") ?? "";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// 状态码页面（404/403 等）— 由 UseStatusCodePagesWithReExecute 中间件调用
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult HandleStatusCode(int code)
    {
        ViewBag.StatusCode = code;
        return code switch
        {
            404 => View("NotFound"),
            403 => View("Forbidden"),
            _ => View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier })
        };
    }
}
