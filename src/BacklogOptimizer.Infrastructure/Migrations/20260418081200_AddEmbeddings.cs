using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace BacklogOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "JiraIssueEmbeddings",
                columns: table => new
                {
                    JiraIssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    Vector = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    EmbeddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JiraIssueEmbeddings", x => x.JiraIssueId);
                    table.ForeignKey(
                        name: "FK_JiraIssueEmbeddings_JiraIssues_JiraIssueId",
                        column: x => x.JiraIssueId,
                        principalTable: "JiraIssues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScrapedPageEmbeddings",
                columns: table => new
                {
                    ScrapedPageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    Vector = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    EmbeddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapedPageEmbeddings", x => x.ScrapedPageId);
                    table.ForeignKey(
                        name: "FK_ScrapedPageEmbeddings_ScrapedPages_ScrapedPageId",
                        column: x => x.ScrapedPageId,
                        principalTable: "ScrapedPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JiraIssueEmbeddings");

            migrationBuilder.DropTable(
                name: "ScrapedPageEmbeddings");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
