using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySeatReservation.Web.Data;
using LibrarySeatReservation.Web.Filters;
using LibrarySeatReservation.Web.Helpers;
using LibrarySeatReservation.Web.Models;

namespace LibrarySeatReservation.Web.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    // ========== 认证 ==========

    /// <summary>
    /// 管理员登录（GET）— 已登录则跳转预约管理页
    /// </summary>
    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetString("AdminLoggedIn") == "true")
            return RedirectToAction("Reservations");

        return View();
    }

    /// <summary>
    /// 管理员登录（POST）— 校验 admin/admin123
    /// </summary>
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

    /// <summary>
    /// 管理员登出（POST）— 清除 Session
    /// </summary>
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("AdminLoggedIn");
        HttpContext.Session.Remove("AdminName");
        return RedirectToAction("Login");
    }

    // ========== 预约管理 ==========

    /// <summary>
    /// 预约管理 — 查询全部预约记录
    /// </summary>
    [AdminAuth]
    public IActionResult Reservations()
    {
        var reservations = _db.Reservations
            .Include(r => r.Seat)
            .Include(r => r.StudentUser)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.StartTime)
            .ToList();

        ViewBag.Reservations = reservations.Select(r => new
        {
            r.Id,
            SeatNumber = r.Seat.SeatNumber,
            r.Seat.Area,
            UserName = r.StudentUser.Name,
            r.Date,
            r.StartTime,
            r.EndTime,
            r.Status,
            DisplayStatus = StatusHelper.GetReservationDisplayStatus(r.Status, r.StartTime, r.EndTime),
            r.CreatedAt
        }).ToList();

        return View();
    }

    // ========== 座位管理 ==========

    /// <summary>
    /// 座位管理 — 查询全部座位 + 添加表单
    /// </summary>
    [AdminAuth]
    public IActionResult Seats()
    {
        var seats = _db.Seats.OrderBy(s => s.SeatNumber).ToList();
        return View(seats);
    }

    /// <summary>
    /// 添加座位（POST）
    /// </summary>
    [AdminAuth]
    [HttpPost]
    public IActionResult CreateSeat(string seatNumber, string area, string floor)
    {
        if (string.IsNullOrWhiteSpace(seatNumber) || string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(floor))
        {
            TempData["ErrorMessage"] = "请填写所有字段";
            return RedirectToAction("Seats");
        }

        // 唯一性校验
        var exists = _db.Seats.Any(s => s.SeatNumber == seatNumber);
        if (exists)
        {
            TempData["ErrorMessage"] = $"座位编号 {seatNumber} 已存在";
            return RedirectToAction("Seats");
        }

        _db.Seats.Add(new Seat
        {
            SeatNumber = seatNumber,
            Area = area,
            Floor = floor,
            Status = "Available"
        });
        _db.SaveChanges();

        TempData["SuccessMessage"] = $"座位 {seatNumber} 已添加";
        return RedirectToAction("Seats");
    }

    // ========== 统计 ==========

    /// <summary>
    /// 统计页 — 实时计算统计数据
    /// </summary>
    [AdminAuth]
    public IActionResult Stats()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var totalSeats = _db.Seats.Count();

        // 今日预约数（所有状态）
        var todayReservations = _db.Reservations.Count(r => r.Date == today);

        // 座位使用率：今日有 Pending 预约的座位数 / 座位总数
        var distinctSeatCount = _db.Reservations
            .Where(r => r.Date == today && r.Status == "Pending")
            .Select(r => r.SeatId)
            .Distinct()
            .Count();

        var usageRate = totalSeats > 0
            ? (double)distinctSeatCount / totalSeats * 100
            : 0;

        // 各区域座位分布
        var areaDistribution = _db.Seats
            .GroupBy(s => s.Area)
            .Select(g => new { Area = g.Key, Count = g.Count() })
            .ToList();

        ViewBag.TotalSeats = totalSeats;
        ViewBag.TodayReservations = todayReservations;
        ViewBag.UsageRate = Math.Round(usageRate, 1);
        ViewBag.AreaDistribution = areaDistribution;

        return View();
    }
}
