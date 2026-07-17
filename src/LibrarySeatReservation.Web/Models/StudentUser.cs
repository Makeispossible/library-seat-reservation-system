namespace LibrarySeatReservation.Web.Models;

/// <summary>
/// 用户实体 — 对应 StudentUsers 表（含注册/登录信息）
/// </summary>
public class StudentUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }

    // Navigation
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
