namespace LibrarySeatReservation.Web.Models;

/// <summary>
/// 视图模型 — 座位列表页展示对象
/// DisplayStatus 为 StatusHelper 计算后的动态状态字符串
/// </summary>
public class SeatDisplay
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string DisplayStatus { get; set; } = string.Empty;
}
