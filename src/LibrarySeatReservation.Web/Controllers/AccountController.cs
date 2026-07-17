using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LibrarySeatReservation.Web.Data;
using LibrarySeatReservation.Web.Models;

namespace LibrarySeatReservation.Web.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<StudentUser> _passwordHasher = new();

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    // ========== 注册 ==========

    [HttpGet]
    public IActionResult Register()
    {
        // 已登录用户跳转首页
        if (HttpContext.Session.GetInt32("UserId").HasValue)
            return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // 检查用户名唯一性
        var exists = _db.StudentUsers.Any(u => u.Username == model.Username);
        if (exists)
        {
            ModelState.AddModelError("Username", "该用户名已被注册");
            return View(model);
        }

        // 创建用户
        var user = new StudentUser
        {
            Username = model.Username,
            Name = model.Name
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _db.StudentUsers.Add(user);
        _db.SaveChanges();

        // 自动登录
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);

        TempData["SuccessMessage"] = "注册成功，欢迎使用！";
        return RedirectToAction("Index", "Home");
    }

    // ========== 登录 ==========

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (HttpContext.Session.GetInt32("UserId").HasValue)
            return RedirectToAction("Index", "Home");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = _db.StudentUsers.FirstOrDefault(u => u.Username == model.Username);
        if (user == null || user.PasswordHash == null)
        {
            ModelState.AddModelError("", "用户名或密码错误");
            return View(model);
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "用户名或密码错误");
            return View(model);
        }

        // 登录成功 — 设置 Session
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);

        // 如果有来源页，跳回来源页
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        TempData["SuccessMessage"] = "登录成功！";
        return RedirectToAction("Index", "Home");
    }

    // ========== 登出 ==========

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("UserId");
        HttpContext.Session.Remove("UserName");
        TempData["SuccessMessage"] = "已退出登录";
        return RedirectToAction("Index", "Home");
    }
}
