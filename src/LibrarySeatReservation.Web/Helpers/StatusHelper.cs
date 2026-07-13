using LibrarySeatReservation.Web.Models;

namespace LibrarySeatReservation.Web.Helpers;

/// <summary>
/// 动态状态计算工具类
/// 根据 DB 持久化状态 + 当前时间 + 关联预约记录 计算展示状态
/// </summary>
public static class StatusHelper
{
    /// <summary>
    /// 计算座位的展示状态
    /// 结果：空闲 / 已预约 / 使用中 / 维护中
    /// </summary>
    /// <param name="seatStatus">Seat.Status 持久化值</param>
    /// <param name="todayPending">该座位今日所有 Pending 预约</param>
    /// <returns>中文显示状态</returns>
    public static string GetSeatDisplayStatus(string seatStatus, List<Reservation> todayPending)
    {
        if (seatStatus == "Maintenance")
            return "维护中";

        var now = DateTime.Now.TimeOfDay;

        // 检查是否有正在使用中的预约（StartTime ≤ 当前时间 < EndTime）
        if (todayPending.Any(r => r.StartTime <= now && now < r.EndTime))
            return "使用中";

        // 检查是否有即将开始的预约（StartTime > 当前时间）
        if (todayPending.Any(r => r.StartTime > now))
            return "已预约";

        return "空闲";
    }

    /// <summary>
    /// 计算预约记录的展示状态
    /// 结果：待开始 / 使用中 / 已完成 / 已取消
    /// </summary>
    /// <param name="status">Reservation.Status 持久化值</param>
    /// <param name="startTime">预约开始时间</param>
    /// <param name="endTime">预约结束时间</param>
    /// <returns>中文显示状态</returns>
    public static string GetReservationDisplayStatus(string status, TimeSpan startTime, TimeSpan endTime)
    {
        if (status == "Cancelled")
            return "已取消";

        // status 为 "Pending"
        var now = DateTime.Now.TimeOfDay;

        if (now >= endTime)
            return "已完成";

        if (now >= startTime)
            return "使用中";

        return "待开始";
    }
}
