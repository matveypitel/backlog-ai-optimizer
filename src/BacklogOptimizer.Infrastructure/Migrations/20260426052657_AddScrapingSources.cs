using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScrapingSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceId",
                table: "ScrapingJobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScrapingSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastScrapedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RefreshIntervalHours = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapingSources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrapingSources_Url",
                table: "ScrapingSources",
                column: "Url",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScrapingSources");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "ScrapingJobs");
        }
    }
}
