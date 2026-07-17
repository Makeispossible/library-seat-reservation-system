using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySeatReservation.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 幂等：如果因上次失败迁移导致列已存在，先删除
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[StudentUsers]') AND name = 'PasswordHash')
                    ALTER TABLE [StudentUsers] DROP COLUMN [PasswordHash];
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('[StudentUsers]') AND name = 'Username')
                    ALTER TABLE [StudentUsers] DROP COLUMN [Username];
            ");

            // 清理旧版体验账号（通过 Name 字段识别），避免后续唯一索引冲突
            migrationBuilder.Sql("DELETE FROM [Reservations] WHERE StudentUserId IN (SELECT Id FROM [StudentUsers] WHERE Name LIKE N'学生%')");
            migrationBuilder.Sql("DELETE FROM [StudentUsers] WHERE Name LIKE N'学生%'");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "StudentUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "StudentUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_StudentUsers_Username",
                table: "StudentUsers",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StudentUsers_Username",
                table: "StudentUsers");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "StudentUsers");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "StudentUsers");
        }
    }
}
