using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacklogOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJiraUrlAndSuggestionAppliedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AppliedAt",
                table: "ReprioritizationSuggestions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JiraUrl",
                table: "JiraIssues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppliedAt",
                table: "FeatureSuggestions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppliedJiraKey",
                table: "FeatureSuggestions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppliedAt",
                table: "ReprioritizationSuggestions");

            migrationBuilder.DropColumn(
                name: "JiraUrl",
                table: "JiraIssues");

            migrationBuilder.DropColumn(
                name: "AppliedAt",
                table: "FeatureSuggestions");

            migrationBuilder.DropColumn(
                name: "AppliedJiraKey",
                table: "FeatureSuggestions");
        }
    }
}
