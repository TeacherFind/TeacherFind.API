using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherFind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectAndCityToTeacherListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "TeacherProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationLevel",
                table: "TeacherProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsStudent",
                table: "TeacherProfiles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UniversityId",
                table: "TeacherProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Listing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ServiceType",
                table: "Listing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "Listing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Universities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniversityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherProfiles_DepartmentId",
                table: "TeacherProfiles",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherProfiles_UniversityId",
                table: "TeacherProfiles",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_Listing_CityId",
                table: "Listing",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Listing_SubjectId",
                table: "Listing",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_UniversityId",
                table: "Departments",
                column: "UniversityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Listing_Cities_CityId",
                table: "Listing",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Listing_Subjects_SubjectId",
                table: "Listing",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherProfiles_Departments_DepartmentId",
                table: "TeacherProfiles",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherProfiles_Universities_UniversityId",
                table: "TeacherProfiles",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listing_Cities_CityId",
                table: "Listing");

            migrationBuilder.DropForeignKey(
                name: "FK_Listing_Subjects_SubjectId",
                table: "Listing");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherProfiles_Departments_DepartmentId",
                table: "TeacherProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherProfiles_Universities_UniversityId",
                table: "TeacherProfiles");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_TeacherProfiles_DepartmentId",
                table: "TeacherProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TeacherProfiles_UniversityId",
                table: "TeacherProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Listing_CityId",
                table: "Listing");

            migrationBuilder.DropIndex(
                name: "IX_Listing_SubjectId",
                table: "Listing");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "EducationLevel",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "IsStudent",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "UniversityId",
                table: "TeacherProfiles");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Listing");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "Listing");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Listing");
        }
    }
}
