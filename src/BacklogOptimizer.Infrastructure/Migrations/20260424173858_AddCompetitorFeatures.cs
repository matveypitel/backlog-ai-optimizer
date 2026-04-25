using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace BacklogOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitorFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScrapedPageEmbeddings");

            migrationBuilder.CreateTable(
                name: "CompetitorFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScrapedPageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    ExtractedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitorFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitorFeatures_ScrapedPages_ScrapedPageId",
                        column: x => x.ScrapedPageId,
                        principalTable: "ScrapedPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetitorFeatureEmbeddings",
                columns: table => new
                {
                    CompetitorFeatureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    Vector = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    EmbeddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitorFeatureEmbeddings", x => x.CompetitorFeatureId);
                    table.ForeignKey(
                        name: "FK_CompetitorFeatureEmbeddings_CompetitorFeatures_CompetitorFe~",
                        column: x => x.CompetitorFeatureId,
                        principalTable: "CompetitorFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorFeatures_ScrapedPageId",
                table: "CompetitorFeatures",
                column: "ScrapedPageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitorFeatureEmbeddings");

            migrationBuilder.DropTable(
                name: "CompetitorFeatures");

            migrationBuilder.CreateTable(
                name: "ScrapedPageEmbeddings",
                columns: table => new
                {
                    ScrapedPageId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmbeddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModelName = table.Column<string>(type: "text", nullable: false),
                    Vector = table.Column<Vector>(type: "vector(1536)", nullable: false)
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
    }
}
