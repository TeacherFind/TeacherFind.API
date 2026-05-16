using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherFind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherProfileMediaAndAvailabilityRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAvailability_TeacherProfiles_TeacherProfileId",
                table: "TeacherAvailability");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeacherAvailability",
                table: "TeacherAvailability");

            migrationBuilder.RenameTable(
                name: "TeacherAvailability",
                newName: "TeacherAvailabilities");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherAvailability_TeacherProfileId",
                table: "TeacherAvailabilities",
                newName: "IX_TeacherAvailabilities_TeacherProfileId");

            migrationBuilder.AlterColumn<string>(
                name: "Organization",
                table: "TeacherCertificates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TeacherCertificates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FileUrl",
                table: "TeacherCertificates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "TeacherCertificates",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "TeacherCertificates",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Start",
                table: "TeacherAvailabilities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "End",
                table: "TeacherAvailabilities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Day",
                table: "TeacherAvailabilities",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeacherAvailabilities",
                table: "TeacherAvailabilities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAvailabilities_TeacherProfiles_TeacherProfileId",
                table: "TeacherAvailabilities",
                column: "TeacherProfileId",
                principalTable: "TeacherProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherAvailabilities_TeacherProfiles_TeacherProfileId",
                table: "TeacherAvailabilities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeacherAvailabilities",
                table: "TeacherAvailabilities");

            migrationBuilder.RenameTable(
                name: "TeacherAvailabilities",
                newName: "TeacherAvailability");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherAvailabilities_TeacherProfileId",
                table: "TeacherAvailability",
                newName: "IX_TeacherAvailability_TeacherProfileId");

            migrationBuilder.AlterColumn<string>(
                name: "Organization",
                table: "TeacherCertificates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TeacherCertificates",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "FileUrl",
                table: "TeacherCertificates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "TeacherCertificates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentType",
                table: "TeacherCertificates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Start",
                table: "TeacherAvailability",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "End",
                table: "TeacherAvailability",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Day",
                table: "TeacherAvailability",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeacherAvailability",
                table: "TeacherAvailability",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherAvailability_TeacherProfiles_TeacherProfileId",
                table: "TeacherAvailability",
                column: "TeacherProfileId",
                principalTable: "TeacherProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
