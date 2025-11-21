using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadReview.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixedIGDBConstrains : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Platforms_Generation",
                table: "Platforms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Platforms_PlatformType",
                table: "Platforms");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Games_RatingIGDB",
                table: "Games");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Developers_Country",
                table: "Developers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Developers_StartDate",
                table: "Developers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Platforms_Generation",
                table: "Platforms",
                sql: "[Generation] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Platforms_PlatformType",
                table: "Platforms",
                sql: "[PlatformType] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Games_RatingIGDB",
                table: "Games",
                sql: "[RatingIGDB] >= 0 AND [RatingIGDB] <= 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Developers_Country",
                table: "Developers",
                sql: "[Country] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Developers_StartDate",
                table: "Developers",
                sql: "[StartDate] >= 0");
        }
    }
}
