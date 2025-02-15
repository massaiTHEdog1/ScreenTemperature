using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenTemperature.Migrations
{
    /// <inheritdoc />
    public partial class applyconfigurationatstartup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ApplyAtStartup",
                table: "Configurations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplyAtStartup",
                table: "Configurations");
        }
    }
}
