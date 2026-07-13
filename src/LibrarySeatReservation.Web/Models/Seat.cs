namespace LibrarySeatReservation.Web.Models;

/// <summary>
/// 座位实体 — 对应 Seats 表
/// Status 持久化状态：Available / Maintenance
/// </summary>
public class Seat
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";

    // Navigation
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
