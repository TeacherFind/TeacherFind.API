using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherFind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherCertificateFileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherCertificate_TeacherProfiles_TeacherProfileId",
                table: "TeacherCertificate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeacherCertificate",
                table: "TeacherCertificate");

            migrationBuilder.RenameTable(
                name: "TeacherCertificate",
                newName: "TeacherCertificates");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherCertificate_TeacherProfileId",
                table: "TeacherCertificates",
                newName: "IX_TeacherCertificates_TeacherProfileId");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "TeacherCertificates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "TeacherCertificates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "TeacherCertificates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeacherCertificates",
                table: "TeacherCertificates",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherCertificates_TeacherProfiles_TeacherProfileId",
                table: "TeacherCertificates",
                column: "TeacherProfileId",
                principalTable: "TeacherProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherCertificates_TeacherProfiles_TeacherProfileId",
                table: "TeacherCertificates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TeacherCertificates",
                table: "TeacherCertificates");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "TeacherCertificates");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "TeacherCertificates");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "TeacherCertificates");

            migrationBuilder.RenameTable(
                name: "TeacherCertificates",
                newName: "TeacherCertificate");

            migrationBuilder.RenameIndex(
                name: "IX_TeacherCertificates_TeacherProfileId",
                table: "TeacherCertificate",
                newName: "IX_TeacherCertificate_TeacherProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TeacherCertificate",
                table: "TeacherCertificate",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherCertificate_TeacherProfiles_TeacherProfileId",
                table: "TeacherCertificate",
                column: "TeacherProfileId",
                principalTable: "TeacherProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
