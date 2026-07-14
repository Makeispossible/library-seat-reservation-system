using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibrarySeatReservation.Web.Data;
using LibrarySeatReservation.Web.Helpers;
using LibrarySeatReservation.Web.Models;

namespace LibrarySeatReservation.Web.Controllers;

public class SeatsController : Controller
{
    private readonly AppDbContext _db;

    public SeatsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 座位列表 — 支持按区域/楼层筛选
    /// </summary>
    public IActionResult Index(string? area, string? floor)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        // 构建筛选查询
        var query = _db.Seats.AsQueryable();

        if (!string.IsNullOrEmpty(area) && area != "全部")
            query = query.Where(s => s.Area == area);

        if (!string.IsNullOrEmpty(floor) && floor != "全部")
            query = query.Where(s => s.Floor == floor);

        // 今日所有 Pending 预约（用于动态状态计算）
        var todayPending = _db.Reservations
            .Where(r => r.Date == today && r.Status == "Pending")
            .ToList();

        // 组装视图模型
        var seats = query
            .OrderBy(s => s.SeatNumber)
            .AsEnumerable()
            .Select(s =>
            {
                var pendingForSeat = todayPending.Where(r => r.SeatId == s.Id).ToList();
                return new SeatDisplay
                {
                    Id = s.Id,
                    SeatNumber = s.SeatNumber,
                    Area = s.Area,
                    Floor = s.Floor,
                    DisplayStatus = StatusHelper.GetSeatDisplayStatus(s.Status, pendingForSeat)
                };
            })
            .ToList();

        // 传递筛选选项
        ViewBag.Areas = _db.Seats.Select(s => s.Area).Distinct().OrderBy(a => a).ToList();
        ViewBag.Floors = _db.Seats.Select(s => s.Floor).Distinct().OrderBy(f => f).ToList();
        ViewBag.SelectedArea = area ?? "全部";
        ViewBag.SelectedFloor = floor ?? "全部";

        return View(seats);
    }

    /// <summary>
    /// 座位详情 — 显示座位信息 + 当日预约时段列表
    /// </summary>
    public IActionResult Detail(int id)
    {
        var seat = _db.Seats.FirstOrDefault(s => s.Id == id);
        if (seat == null) return NotFound();

        var today = DateOnly.FromDateTime(DateTime.Today);

        // 查询当日预约记录（含预约人信息）
        var todayReservations = _db.Reservations
            .Include(r => r.StudentUser)
            .Where(r => r.SeatId == id && r.Date == today)
            .OrderBy(r => r.StartTime)
            .ToList();

        // 计算座位展示状态
        var pendingForSeat = todayReservations
            .Where(r => r.Status == "Pending")
            .ToList();
        ViewBag.DisplayStatus = StatusHelper.GetSeatDisplayStatus(seat.Status, pendingForSeat);

        // 为每条预约记录计算动态状态
        ViewBag.TodayReservations = todayReservations.Select(r => new
        {
            r.Id,
            UserName = r.StudentUser.Name,
            r.StartTime,
            r.EndTime,
            r.Status,
            DisplayStatus = StatusHelper.GetReservationDisplayStatus(r.Status, r.Date, r.StartTime, r.EndTime)
        }).ToList();

        return View(seat);
    }
}
