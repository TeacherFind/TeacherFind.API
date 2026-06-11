using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherFind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureNotificationSenderColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Notifications', 'SenderUserId') IS NULL
BEGIN
    ALTER TABLE Notifications
    ADD SenderUserId UNIQUEIDENTIFIER NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Notifications', 'SenderName') IS NULL
BEGIN
    ALTER TABLE Notifications
    ADD SenderName NVARCHAR(MAX) NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Notifications', 'Link') IS NULL
BEGIN
    ALTER TABLE Notifications
    ADD Link NVARCHAR(MAX) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Notifications', 'SenderUserId') IS NOT NULL
BEGIN
    ALTER TABLE Notifications
    DROP COLUMN SenderUserId;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Notifications', 'SenderName') IS NOT NULL
BEGIN
    ALTER TABLE Notifications
    DROP COLUMN SenderName;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Notifications', 'Link') IS NOT NULL
BEGIN
    ALTER TABLE Notifications
    DROP COLUMN Link;
END
");
        }
    }
}
