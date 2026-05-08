using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvaloniaNovel.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptTemplateVariables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Variables",
                table: "PromptTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Variables",
                table: "PromptTemplates");
        }
    }
}
