using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadReview.Api.Migrations
{
    /// <inheritdoc />
    public partial class NewConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Developers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Country = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartDate = table.Column<long>(type: "bigint", nullable: true),
                    Logo_ImageId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Logo_ImageHeight = table.Column<int>(type: "int", nullable: true),
                    Logo_ImageWidth = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Developers", x => x.Id);
                    table.CheckConstraint("CK_Developers_Country", "[Country] >= 0");
                    table.CheckConstraint("CK_Developers_StartDate", "[StartDate] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Date = table.Column<long>(type: "bigint", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RatingIGDB = table.Column<double>(type: "float(5)", precision: 5, scale: 2, nullable: false, defaultValue: 0.0),
                    Total_RatingBadReview = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    Count_RatingBadReview = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    Video = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cover_ImageId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cover_ImageHeight = table.Column<int>(type: "int", nullable: true),
                    Cover_ImageWidth = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.CheckConstraint("CK_Games_Count_RatingBadReview", "[Count_RatingBadReview] >= 0");
                    table.CheckConstraint("CK_Games_RatingIGDB", "[RatingIGDB] >= 0 AND [RatingIGDB] <= 100");
                    table.CheckConstraint("CK_Games_Total_RatingBadReview", "[Total_RatingBadReview] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platforms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Generation = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    Summary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PlatformType = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    PlatformTypeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Logo_ImageId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Logo_ImageHeight = table.Column<int>(type: "int", nullable: true),
                    Logo_ImageWidth = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platforms", x => x.Id);
                    table.CheckConstraint("CK_Platforms_Generation", "[Generation] >= 0");
                    table.CheckConstraint("CK_Platforms_PlatformType", "[PlatformType] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Country = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    Date_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    Date_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.CheckConstraint("CK_Users_Birthday_MinAge", "Birthday IS NULL OR DATEDIFF(YEAR, Birthday, GETDATE()) >= 12");
                    table.CheckConstraint("CK_Users_Country", "[Country] >= 0");
                    table.CheckConstraint("CK_Users_Email_Format", "Email NOT LIKE '% %' AND Email LIKE '___%@__%._%'AND (LEN(Email) - LEN(REPLACE(Email, '@', ''))) = 1AND Email NOT LIKE '%[^a-zA-Z0-9@._-]%'");
                    table.CheckConstraint("CK_Users_FullName_AlphaSpace", "FullName IS NULL OR FullName NOT LIKE '%[^a-zA-Z ]%'");
                    table.CheckConstraint("CK_Users_Username_ValidChars", "Username NOT LIKE '%[^a-zA-Z0-9._-]%'");
                });

            migrationBuilder.CreateTable(
                name: "GameDevelopers",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false),
                    DeveloperId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameDevelopers", x => new { x.GameId, x.DeveloperId });
                    table.ForeignKey(
                        name: "FK_GameDevelopers_Developers_DeveloperId",
                        column: x => x.DeveloperId,
                        principalTable: "Developers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameDevelopers_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameGenres",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false),
                    GenreId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameGenres", x => new { x.GameId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_GameGenres_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GamePlatforms",
                columns: table => new
                {
                    GameId = table.Column<int>(type: "int", nullable: false),
                    PlatformId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePlatforms", x => new { x.GameId, x.PlatformId });
                    table.ForeignKey(
                        name: "FK_GamePlatforms_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GamePlatforms_Platforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "Platforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StateEnum = table.Column<int>(type: "int", nullable: false),
                    IsFavorite = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsReview = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Date_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    Date_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.CheckConstraint("CK_Reviews_Dates", "([StartDate] IS NULL OR [EndDate] IS NULL OR [EndDate] >= [StartDate])");
                    table.CheckConstraint("CK_Reviews_Rating", "[Rating] >= 0 AND [Rating] <= 5");
                    table.CheckConstraint("CK_Reviews_ReviewFlags", "([IsReview] = 1 OR [IsFavorite] = 1)");
                    table.CheckConstraint("CK_Reviews_StateDates", "([StateEnum] = 1 AND [EndDate] IS NULL) OR\n            ([StateEnum] = 2 AND [StartDate] IS NULL AND [EndDate] IS NULL) OR\n            ([StateEnum] IN (0,3))");
                    table.ForeignKey(
                        name: "FK_Reviews_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameDevelopers_DeveloperId",
                table: "GameDevelopers",
                column: "DeveloperId");

            migrationBuilder.CreateIndex(
                name: "IX_GameGenres_GenreId",
                table: "GameGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlatforms_PlatformId",
                table: "GamePlatforms",
                column: "PlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_GameId",
                table: "Reviews",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "UX_Reviews_UserId_GameId",
                table: "Reviews",
                columns: new[] { "UserId", "GameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameDevelopers");

            migrationBuilder.DropTable(
                name: "GameGenres");

            migrationBuilder.DropTable(
                name: "GamePlatforms");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Developers");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Platforms");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
