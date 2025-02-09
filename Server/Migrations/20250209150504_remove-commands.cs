using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenTemperature.Migrations
{
    /// <inheritdoc />
    public partial class removecommands : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplyConfigurationCommands");

            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropColumn(
                name: "Shift",
                table: "KeyBindings");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<bool>(
                name: "Shift",
                table: "KeyBindings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyBindingId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commands_KeyBindings_KeyBindingId",
                        column: x => x.KeyBindingId,
                        principalTable: "KeyBindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplyConfigurationCommands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplyConfigurationCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplyConfigurationCommands_Commands_Id",
                        column: x => x.Id,
                        principalTable: "Commands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplyConfigurationCommands_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplyConfigurationCommands_ConfigurationId",
                table: "ApplyConfigurationCommands",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Commands_KeyBindingId",
                table: "Commands",
                column: "KeyBindingId");
        }
    }
}
