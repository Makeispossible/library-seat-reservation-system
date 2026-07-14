namespace LibrarySeatReservation.Web.Models;

/// <summary>
/// 体验账号实体 — 对应 StudentUsers 表
/// </summary>
public class StudentUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
