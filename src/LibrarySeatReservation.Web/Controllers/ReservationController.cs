using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySeatReservation.Web.Data;
using LibrarySeatReservation.Web.Helpers;
using LibrarySeatReservation.Web.Models;

namespace LibrarySeatReservation.Web.Controllers;

public class ReservationController : Controller
{
    private readonly AppDbContext _db;

    public ReservationController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 预约提交（GET）— 显示预约表单
    /// </summary>
    [HttpGet]
    public IActionResult Create(int id)
    {
        // 检查是否已选体验账号
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["ErrorMessage"] = "请先选择体验账号";
            return RedirectToAction("Index", "Home");
        }

        var seat = _db.Seats.FirstOrDefault(s => s.Id == id);
        if (seat == null) return NotFound();

        // 检查座位当前是否空闲
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayPending = _db.Reservations
            .Where(r => r.SeatId == id && r.Date == today && r.Status == "Pending")
            .ToList();

        var displayStatus = StatusHelper.GetSeatDisplayStatus(seat.Status, todayPending);
        if (displayStatus != "空闲")
        {
            TempData["ErrorMessage"] = "该座位当前不可预约";
            return RedirectToAction("Detail", "Seats", new { id });
        }

        ViewBag.Seat = seat;
        return View();
    }

    /// <summary>
    /// 预约提交（POST）— 校验 + 写入 + 重定向
    /// </summary>
    [HttpPost]
    public IActionResult Create(int id, TimeSpan startTime, TimeSpan endTime)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["ErrorMessage"] = "请先选择体验账号";
            return RedirectToAction("Index", "Home");
        }

        var seat = _db.Seats.FirstOrDefault(s => s.Id == id);
        if (seat == null) return NotFound();

        var today = DateOnly.FromDateTime(DateTime.Today);

        // === 时间规则校验 ===
        if (endTime <= startTime)
        {
            ViewBag.Seat = seat;
            ViewBag.Error = "结束时间必须晚于开始时间";
            return View();
        }

        if ((endTime - startTime).TotalMinutes < 30)
        {
            ViewBag.Seat = seat;
            ViewBag.Error = "最短预约时长为 30 分钟";
            return View();
        }

        if ((endTime - startTime).TotalHours > 4)
        {
            ViewBag.Seat = seat;
            ViewBag.Error = "最长预约时长为 4 小时";
            return View();
        }

        // === 冲突检查 ===
        var conflict = _db.Reservations.Any(r =>
            r.SeatId == id &&
            r.Date == today &&
            r.Status == "Pending" &&
            r.StartTime < endTime &&
            r.EndTime > startTime);

        if (conflict)
        {
            ViewBag.Seat = seat;
            ViewBag.Error = "该时段已被预约";
            return View();
        }

        // === 写入 ===
        _db.Reservations.Add(new Reservation
        {
            SeatId = id,
            StudentUserId = userId.Value,
            Date = today,
            StartTime = startTime,
            EndTime = endTime,
            Status = "Pending",
            CreatedAt = DateTime.Now
        });
        _db.SaveChanges();

        TempData["SuccessMessage"] = "预约成功！";
        return RedirectToAction("My");
    }

    /// <summary>
    /// 我的预约 — 显示当前用户的预约记录
    /// </summary>
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

        var nowTime = DateTime.Now.TimeOfDay;

        ViewBag.Reservations = reservations.Select(r => new
        {
            r.Id,
            SeatNumber = r.Seat.SeatNumber,
            r.Seat.Area,
            r.Date,
            r.StartTime,
            r.EndTime,
            DisplayStatus = StatusHelper.GetReservationDisplayStatus(r.Status, r.StartTime, r.EndTime),
            CanCancel = r.Status == "Pending" && nowTime < r.StartTime
        }).ToList();

        return View();
    }

    /// <summary>
    /// 取消预约（POST）— 状态变更为 Cancelled
    /// </summary>
    [HttpPost]
    public IActionResult Cancel(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["ErrorMessage"] = "请先登录";
            return RedirectToAction("Index", "Home");
        }

        var reservation = _db.Reservations
            .Include(r => r.Seat)
            .FirstOrDefault(r => r.Id == id);

        // 边界 1：预约不存在
        if (reservation == null) return NotFound();

        // 边界 2：不是本人的预约
        if (reservation.StudentUserId != userId)
        {
            TempData["ErrorMessage"] = "只能取消自己的预约";
            return RedirectToAction("My");
        }

        // 边界 3：不是待开始状态
        if (reservation.Status != "Pending")
        {
            TempData["ErrorMessage"] = "只能取消待开始的预约";
            return RedirectToAction("My");
        }

        // 边界 4：已经开始
        if (DateTime.Now.TimeOfDay >= reservation.StartTime)
        {
            TempData["ErrorMessage"] = "已开始的预约无法取消";
            return RedirectToAction("My");
        }

        // 执行取消
        reservation.Status = "Cancelled";
        _db.SaveChanges();

        TempData["SuccessMessage"] = "预约已取消";
        return RedirectToAction("My");
    }
}
