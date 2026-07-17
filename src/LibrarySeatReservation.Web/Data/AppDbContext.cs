using Microsoft.EntityFrameworkCore;
using LibrarySeatReservation.Web.Models;

namespace LibrarySeatReservation.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<StudentUser> StudentUsers => Set<StudentUser>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // === StudentUsers ===
        modelBuilder.Entity<StudentUser>(entity =>
        {
            entity.ToTable("StudentUsers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.HasIndex(e => e.Username, "IX_StudentUsers_Username").IsUnique();
        });

        // === Seats ===
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.ToTable("Seats");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SeatNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Area).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Floor).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Available");

            // 索引：按区域/楼层筛选
            entity.HasIndex(e => new { e.Area, e.Floor }, "IX_Seats_Area_Floor");
        });

        // === Reservations ===
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

            // 外键：SeatId → Seats.Id（CASCADE）
            entity.HasOne(e => e.Seat)
                  .WithMany(s => s.Reservations)
                  .HasForeignKey(e => e.SeatId)
                  .OnDelete(DeleteBehavior.Cascade);

            // 外键：StudentUserId → StudentUsers.Id（CASCADE）
            entity.HasOne(e => e.StudentUser)
                  .WithMany(u => u.Reservations)
                  .HasForeignKey(e => e.StudentUserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // 索引：预约冲突检查
            entity.HasIndex(e => new { e.SeatId, e.Date }, "IX_Reservations_SeatId_Date");
            // 索引：我的预约查询
            entity.HasIndex(e => e.StudentUserId, "IX_Reservations_StudentUserId");
            // 索引：管理端按时间倒序
            entity.HasIndex(e => new { e.Date, e.StartTime }, "IX_Reservations_Date_StartTime");
        });
    }
}
