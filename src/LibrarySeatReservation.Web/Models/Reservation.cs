namespace LibrarySeatReservation.Web.Models;

/// <summary>
/// 预约记录实体 — 对应 Reservations 表
/// Status 持久化状态：Pending / Cancelled
/// </summary>
public class Reservation
{
    public int Id { get; set; }
    public int SeatId { get; set; }
    public int StudentUserId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Seat Seat { get; set; } = null!;
    public StudentUser StudentUser { get; set; } = null!;
}
