using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadReview.Api.Migrations
{
    /// <inheritdoc />
    public partial class CachedGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingBadReview",
                table: "Games");

            migrationBuilder.AlterColumn<long>(
                name: "Date",
                table: "Games",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Count_RatingBadReview",
                table: "Games",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Total_RatingBadReview",
                table: "Games",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count_RatingBadReview",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Total_RatingBadReview",
                table: "Games");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Games",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RatingBadReview",
                table: "Games",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
