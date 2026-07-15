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
    /// 预约管理 — 查询全部预约记录，支持按日期/状态/用户筛选
    /// </summary>
    [AdminAuth]
    public IActionResult Reservations(string? dateFilter, string? statusFilter, int? userId)
    {
        var query = _db.Reservations
            .Include(r => r.Seat)
            .Include(r => r.StudentUser)
            .AsQueryable();

        // 日期筛选
        if (!string.IsNullOrEmpty(dateFilter) && DateOnly.TryParse(dateFilter, out var parsedDate))
        {
            query = query.Where(r => r.Date == parsedDate);
        }

        // 状态筛选（按持久化状态 + 动态状态需在内存中处理）
        // 先查全部，然后按状态筛选在内存中做
        var reservations = query
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.StartTime)
            .ToList();

        // 映射为显示对象
        var displayList = reservations.Select(r =>
        {
            var displayStatus = StatusHelper.GetReservationDisplayStatus(r.Status, r.Date, r.StartTime, r.EndTime);
            return new
            {
                r.Id,
                SeatNumber = r.Seat.SeatNumber,
                r.Seat.Area,
                UserName = r.StudentUser.Name,
                r.Date,
                r.StartTime,
                r.EndTime,
                r.Status,
                DisplayStatus = displayStatus,
                r.CreatedAt,
                // 可取消条件：状态为 Pending 且（未来日期 或 今天尚未开始）
                CanCancelAdmin = r.Status == "Pending" && (r.Date > DateOnly.FromDateTime(DateTime.Today) || (r.Date == DateOnly.FromDateTime(DateTime.Today) && DateTime.Now.TimeOfDay < r.StartTime))
            };
        }).ToList();

        // 按动态状态筛选（如果有）
        if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "全部")
        {
            displayList = displayList.Where(r => r.DisplayStatus == statusFilter).ToList();
        }

        // 按用户筛选
        if (userId.HasValue)
        {
            displayList = displayList.Where(r => reservations.Any(rr => rr.Id == r.Id && rr.StudentUserId == userId.Value)).ToList();
            // 重新筛选：用 reservations 的原始 StudentUserId
            displayList = displayList.Join(
                reservations.Where(r => r.StudentUserId == userId.Value),
                dl => dl.Id,
                r => r.Id,
                (dl, _) => dl
            ).ToList();
        }

        ViewBag.Reservations = displayList;
        ViewBag.DateFilter = dateFilter ?? "";
        ViewBag.StatusFilter = statusFilter ?? "全部";
        ViewBag.UserId = userId;

        // 传递筛选下拉数据
        ViewBag.StudentUsers = _db.StudentUsers.OrderBy(u => u.Id).ToList();

        return View();
    }

    /// <summary>
    /// 管理员取消预约（POST）— 管理员可取消任意 Pending 预约
    /// </summary>
    [AdminAuth]
    [HttpPost]
    public IActionResult CancelReservation(int id)
    {
        var reservation = _db.Reservations.FirstOrDefault(r => r.Id == id);
        if (reservation == null) return NotFound();

        if (reservation.Status != "Pending")
        {
            TempData["ErrorMessage"] = "只能取消待开始的预约";
            return RedirectToAction("Reservations");
        }

        if (reservation.Date == DateOnly.FromDateTime(DateTime.Today) && DateTime.Now.TimeOfDay >= reservation.StartTime)
        {
            TempData["ErrorMessage"] = "已开始的预约无法取消";
            return RedirectToAction("Reservations");
        }

        reservation.Status = "Cancelled";
        _db.SaveChanges();

        TempData["SuccessMessage"] = $"预约 #{id} 已取消";
        return RedirectToAction("Reservations");
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

    /// <summary>
    /// 切换座位状态（POST）— Available ↔ Maintenance
    /// </summary>
    [AdminAuth]
    [HttpPost]
    public IActionResult ToggleSeatStatus(int id)
    {
        var seat = _db.Seats.FirstOrDefault(s => s.Id == id);
        if (seat == null) return NotFound();

        seat.Status = seat.Status == "Available" ? "Maintenance" : "Available";
        _db.SaveChanges();

        var newStatus = seat.Status == "Available" ? "可用" : "维护中";
        TempData["SuccessMessage"] = $"座位 {seat.SeatNumber} 已切换为「{newStatus}」";
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

        // 各状态座位数（用于详细统计）
        var availableSeats = _db.Seats.Count(s => s.Status == "Available");
        var maintenanceSeats = _db.Seats.Count(s => s.Status == "Maintenance");

        ViewBag.TotalSeats = totalSeats;
        ViewBag.TodayReservations = todayReservations;
        ViewBag.UsageRate = Math.Round(usageRate, 1);
        ViewBag.AreaDistribution = areaDistribution;
        ViewBag.AvailableSeats = availableSeats;
        ViewBag.MaintenanceSeats = maintenanceSeats;

        return View();
    }

    /// <summary>
    /// 区域分布图表数据（JSON）— 供 Chart.js 使用
    /// </summary>
    [AdminAuth]
    [HttpGet]
    public IActionResult GetAreaChartData()
    {
        var data = _db.Seats
            .GroupBy(s => s.Area)
            .Select(g => new { label = g.Key, value = g.Count() })
            .ToList();
        return Json(data);
    }

    /// <summary>
    /// 近 7 日预约趋势数据（JSON）— 供 Chart.js 使用
    /// </summary>
    [AdminAuth]
    [HttpGet]
    public IActionResult GetDailyTrendData()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var data = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = today.AddDays(-(6 - offset));
                var count = _db.Reservations.Count(r => r.Date == date);
                return new { date = date.ToString("MM/dd"), count };
            })
            .ToList();
        return Json(data);
    }
}
