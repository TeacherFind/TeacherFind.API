using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherFind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrationAdi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Reviews', 'BookingId') IS NULL
BEGIN
    ALTER TABLE Reviews
    ADD BookingId UNIQUEIDENTIFIER NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Reviews', 'TeacherProfileId') IS NULL
BEGIN
    ALTER TABLE Reviews
    ADD TeacherProfileId UNIQUEIDENTIFIER NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('Reviews', 'BookingId') IS NOT NULL
BEGIN
    ALTER TABLE Reviews
    DROP COLUMN BookingId;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('Reviews', 'TeacherProfileId') IS NOT NULL
BEGIN
    ALTER TABLE Reviews
    DROP COLUMN TeacherProfileId;
END
");
        }
    }
}