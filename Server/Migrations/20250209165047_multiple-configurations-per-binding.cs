using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenTemperature.Migrations
{
    /// <inheritdoc />
    public partial class multipleconfigurationsperbinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KeyBindings_Configurations_ConfigurationId",
                table: "KeyBindings");

            migrationBuilder.DropIndex(
                name: "IX_KeyBindings_ConfigurationId",
                table: "KeyBindings");

            migrationBuilder.DropColumn(
                name: "ConfigurationId",
                table: "KeyBindings");

            migrationBuilder.CreateTable(
                name: "ConfigurationKeyBinding",
                columns: table => new
                {
                    ConfigurationsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyBindingsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationKeyBinding", x => new { x.ConfigurationsId, x.KeyBindingsId });
                    table.ForeignKey(
                        name: "FK_ConfigurationKeyBinding_Configurations_ConfigurationsId",
                        column: x => x.ConfigurationsId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfigurationKeyBinding_KeyBindings_KeyBindingsId",
                        column: x => x.KeyBindingsId,
                        principalTable: "KeyBindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationKeyBinding_KeyBindingsId",
                table: "ConfigurationKeyBinding",
                column: "KeyBindingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigurationKeyBinding");

            migrationBuilder.AddColumn<Guid>(
                name: "ConfigurationId",
                table: "KeyBindings",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_KeyBindings_ConfigurationId",
                table: "KeyBindings",
                column: "ConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_KeyBindings_Configurations_ConfigurationId",
                table: "KeyBindings",
                column: "ConfigurationId",
                principalTable: "Configurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
