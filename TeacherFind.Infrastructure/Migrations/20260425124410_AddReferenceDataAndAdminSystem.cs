using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherFind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceDataAndAdminSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "TeacherProfiles",
                type: "uniqueidentifier",
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

            migrationBuilder.AddColumn<Guid>(
                name: "UniversityId",
                table: "TeacherProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TeacherListings",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SubCategory",
                table: "TeacherListings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TeacherListings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TeacherListings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "TeacherListings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                table: "TeacherListings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                table: "TeacherListings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NeighborhoodId",
                table: "TeacherListings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "TeacherListings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Listing",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CityId1",
                table: "Listing",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlateCode = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CityPlateCode = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    UniversityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Neighborhoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DistrictId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Neighborhoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Neighborhoods_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "IX_TeacherListings_CityId",
                table: "TeacherListings",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherListings_DistrictId",
                table: "TeacherListings",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherListings_IsActive",
                table: "TeacherListings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherListings_IsApproved",
                table: "TeacherListings",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherListings_NeighborhoodId",
                table: "TeacherListings",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherListings_SubjectId",
                table: "TeacherListings",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Listing_CityId1",
                table: "Listing",
                column: "CityId1");

            migrationBuilder.CreateIndex(
                name: "IX_Listing_SubjectId",
                table: "Listing",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_UniversityId_Name",
                table: "Departments",
                columns: new[] { "UniversityId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_CityId_Name",
                table: "Districts",
                columns: new[] { "CityId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_Code",
                table: "Districts",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_Code",
                table: "Neighborhoods",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_DistrictId_Name",
                table: "Neighborhoods",
                columns: new[] { "DistrictId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Universities_Code",
                table: "Universities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Universities_Name",
                table: "Universities",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Listing_Cities_CityId1",
                table: "Listing",
                column: "CityId1",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Listing_Subjects_SubjectId",
                table: "Listing",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherListings_Cities_CityId",
                table: "TeacherListings",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherListings_Districts_DistrictId",
                table: "TeacherListings",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherListings_Neighborhoods_NeighborhoodId",
                table: "TeacherListings",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherListings_Subjects_SubjectId",
                table: "TeacherListings",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherProfiles_Departments_DepartmentId",
                table: "TeacherProfiles",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherProfiles_Universities_UniversityId",
                table: "TeacherProfiles",
                column: "UniversityId",
                principalTable: "Universities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Listing_Cities_CityId1",
                table: "Listing");

            migrationBuilder.DropForeignKey(
                name: "FK_Listing_Subjects_SubjectId",
                table: "Listing");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherListings_Cities_CityId",
                table: "TeacherListings");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherListings_Districts_DistrictId",
                table: "TeacherListings");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherListings_Neighborhoods_NeighborhoodId",
                table: "TeacherListings");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherListings_Subjects_SubjectId",
                table: "TeacherListings");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherProfiles_Departments_DepartmentId",
                table: "TeacherProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherProfiles_Universities_UniversityId",
                table: "TeacherProfiles");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Neighborhoods");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Universities");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropIndex(
                name: "IX_TeacherProfiles_DepartmentId",
                table: "TeacherProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TeacherProfiles_UniversityId",
                table: "TeacherProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TeacherListings_CityId",
                table: "TeacherListings");

            migrationBuilder.DropIndex(
                name: "IX_TeacherListings_DistrictId",
                table: "TeacherListings");

            migrationBuilder.DropIndex(
                name: "IX_TeacherListings_IsActive",
                table: "TeacherListings");

            migrationBuilder.DropIndex(
                name: "IX_TeacherListings_IsApproved",
                table: "TeacherListings");

            migrationBuilder.DropIndex(
                name: "IX_TeacherListings_NeighborhoodId",
                table: "TeacherListings");

            migrationBuilder.DropIndex(
                name: "IX_TeacherListings_SubjectId",
                table: "TeacherListings");

            migrationBuilder.DropIndex(
                name: "IX_Listing_CityId1",
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
                table: "TeacherListings");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "TeacherListings");

            migrationBuilder.DropColumn(
                name: "NeighborhoodId",
                table: "TeacherListings");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "TeacherListings");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Listing");

            migrationBuilder.DropColumn(
                name: "CityId1",
                table: "Listing");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "Listing");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "Listing");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TeacherListings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "SubCategory",
                table: "TeacherListings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TeacherListings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "TeacherListings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "TeacherListings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
