using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherFind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RebuildSubjectsAsIntIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TeacherListings -> Subjects foreign key varsa kaldır
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_TeacherListings_Subjects_SubjectId'
)
BEGIN
    ALTER TABLE [TeacherListings] DROP CONSTRAINT [FK_TeacherListings_Subjects_SubjectId];
END
");

            // TeacherListings.SubjectId index varsa kaldır
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_TeacherListings_SubjectId'
      AND object_id = OBJECT_ID('[TeacherListings]')
)
BEGIN
    DROP INDEX [IX_TeacherListings_SubjectId] ON [TeacherListings];
END
");

            // TeacherListings.SubjectId kolonu varsa kaldır
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE name = 'SubjectId'
      AND object_id = OBJECT_ID('[TeacherListings]')
)
BEGIN
    ALTER TABLE [TeacherListings] DROP COLUMN [SubjectId];
END
");

            // Eski Subjects tablosu varsa kaldır
            migrationBuilder.Sql(@"
IF OBJECT_ID('[Subjects]', 'U') IS NOT NULL
BEGIN
    DROP TABLE [Subjects];
END
");

            // Subjects tablosunu int identity Id ile yeniden oluştur
            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Code = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Category = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    Name = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false),

                    Level = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false),

                    IsActive = table.Column<bool>(
                        type: "bit",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Code",
                table: "Subjects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Category",
                table: "Subjects",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Level",
                table: "Subjects",
                column: "Level");

            // TeacherListings.SubjectId kolonunu int nullable olarak yeniden ekle
            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "TeacherListings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherListings_SubjectId",
                table: "TeacherListings",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherListings_Subjects_SubjectId",
                table: "TeacherListings",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "Subjects tablosu Guid'den int identity yapıya dönüştürüldüğü için otomatik rollback desteklenmiyor.");
        }
    }
}