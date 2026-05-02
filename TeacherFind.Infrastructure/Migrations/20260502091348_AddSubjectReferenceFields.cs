using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherFind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectReferenceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Subjects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Subjects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Code",
                table: "Subjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Subjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Subjects",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "BookingId",
                table: "Reviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherProfileId",
                table: "Reviews",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SenderUserId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Category",
                table: "Subjects",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Code",
                table: "Subjects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Level",
                table: "Subjects",
                column: "Level");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_Category",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_Code",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_Level",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "TeacherProfileId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Link",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "SenderUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReminderSentAt",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Subjects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
