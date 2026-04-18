using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherFind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateListingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExperienceYears",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "HourlyPrice",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "TeacherProfiles");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "TeacherProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "TeacherProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "TeacherListings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LessonDuration",
                table: "TeacherListings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TeacherListings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "TeacherListings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TeacherAvailability",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Day = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Start = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    End = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAvailability", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherAvailability_TeacherProfiles_TeacherProfileId",
                        column: x => x.TeacherProfileId,
                        principalTable: "TeacherProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherCertificate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Organization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherCertificate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherCertificate_TeacherProfiles_TeacherProfileId",
                        column: x => x.TeacherProfileId,
                        principalTable: "TeacherProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAvailability_TeacherProfileId",
                table: "TeacherAvailability",
                column: "TeacherProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherCertificate_TeacherProfileId",
                table: "TeacherCertificate",
                column: "TeacherProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherAvailability");

            migrationBuilder.DropTable(
                name: "TeacherCertificate");

            migrationBuilder.DropColumn(
                name: "City",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "TeacherListings");

            migrationBuilder.DropColumn(
                name: "LessonDuration",
                table: "TeacherListings");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TeacherListings");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "TeacherListings");

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYears",
                table: "TeacherProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyPrice",
                table: "TeacherProfiles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "TeacherProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
