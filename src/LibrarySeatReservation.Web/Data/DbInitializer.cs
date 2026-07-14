using LibrarySeatReservation.Web.Models;

namespace LibrarySeatReservation.Web.Data;

/// <summary>
/// 数据库初始化器 — 首次运行时写入种子数据
/// </summary>
public static class DbInitializer
{
    public static void Initialize(AppDbContext db)
    {
        // 如果数据库已创建且有数据，跳过
        if (db.StudentUsers.Any()) return;

        // === 种子数据：5 个体验学生 ===
        var students = new List<StudentUser>
        {
            new() { Name = "学生A" },
            new() { Name = "学生B" },
            new() { Name = "学生C" },
            new() { Name = "学生D" },
            new() { Name = "学生E" },
        };
        db.StudentUsers.AddRange(students);
        db.SaveChanges();

        // === 种子数据：12 个座位（3 区域 × 4 座位） ===
        var seats = new List<Seat>
        {
            // 一楼大厅
            new() { SeatNumber = "A-01", Area = "一楼大厅", Floor = "1F", Status = "Available" },
            new() { SeatNumber = "A-02", Area = "一楼大厅", Floor = "1F", Status = "Available" },
            new() { SeatNumber = "A-03", Area = "一楼大厅", Floor = "1F", Status = "Available" },
            new() { SeatNumber = "A-04", Area = "一楼大厅", Floor = "1F", Status = "Available" },
            // 二楼阅览室
            new() { SeatNumber = "B-01", Area = "二楼阅览室", Floor = "2F", Status = "Available" },
            new() { SeatNumber = "B-02", Area = "二楼阅览室", Floor = "2F", Status = "Available" },
            new() { SeatNumber = "B-03", Area = "二楼阅览室", Floor = "2F", Status = "Available" },
            new() { SeatNumber = "B-04", Area = "二楼阅览室", Floor = "2F", Status = "Available" },
            // 三楼自习区
            new() { SeatNumber = "C-01", Area = "三楼自习区", Floor = "3F", Status = "Available" },
            new() { SeatNumber = "C-02", Area = "三楼自习区", Floor = "3F", Status = "Available" },
            new() { SeatNumber = "C-03", Area = "三楼自习区", Floor = "3F", Status = "Available" },
            new() { SeatNumber = "C-04", Area = "三楼自习区", Floor = "3F", Status = "Available" },
        };
        db.Seats.AddRange(seats);
        db.SaveChanges();
    }
}
