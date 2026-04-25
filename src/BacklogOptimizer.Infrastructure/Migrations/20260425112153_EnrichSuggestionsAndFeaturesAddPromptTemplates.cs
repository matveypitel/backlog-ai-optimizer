using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnrichSuggestionsAndFeaturesAddPromptTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcceptanceCriteria",
                table: "FeatureSuggestions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BusinessValue",
                table: "FeatureSuggestions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EstimatedImpact",
                table: "FeatureSuggestions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserStories",
                table: "FeatureSuggestions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Differentiators",
                table: "CompetitorFeatures",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KeyBenefits",
                table: "CompetitorFeatures",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetAudience",
                table: "CompetitorFeatures",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UseCases",
                table: "CompetitorFeatures",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PromptTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_Type",
                table: "PromptTemplates",
                column: "Type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptTemplates");

            migrationBuilder.DropColumn(
                name: "AcceptanceCriteria",
                table: "FeatureSuggestions");

            migrationBuilder.DropColumn(
                name: "BusinessValue",
                table: "FeatureSuggestions");

            migrationBuilder.DropColumn(
                name: "EstimatedImpact",
                table: "FeatureSuggestions");

            migrationBuilder.DropColumn(
                name: "UserStories",
                table: "FeatureSuggestions");

            migrationBuilder.DropColumn(
                name: "Differentiators",
                table: "CompetitorFeatures");

            migrationBuilder.DropColumn(
                name: "KeyBenefits",
                table: "CompetitorFeatures");

            migrationBuilder.DropColumn(
                name: "TargetAudience",
                table: "CompetitorFeatures");

            migrationBuilder.DropColumn(
                name: "UseCases",
                table: "CompetitorFeatures");
        }
    }
}
