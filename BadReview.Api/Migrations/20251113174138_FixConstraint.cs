using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadReview.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_StateDates",
                table: "Reviews");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_StateDates",
                table: "Reviews",
                sql: "([StateEnum] = 1 AND [EndDate] IS NULL) OR\r\n            ([StateEnum] = 2 AND [StartDate] IS NULL AND [EndDate] IS NULL) OR\r\n            ([StateEnum] IN (0,3))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_StateDates",
                table: "Reviews");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_StateDates",
                table: "Reviews",
                sql: "([StateEnum] = 1 AND [EndDate] IS NULL) OR\n            ([StateEnum] = 2 AND [StartDate] IS NULL AND [EndDate] IS NULL) OR\n            ([StateEnum] IN (0,3))");
        }
    }
}
