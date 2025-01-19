using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenTemperature.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DevicePath = table.Column<string>(type: "TEXT", nullable: false),
                    ApplyBrightness = table.Column<bool>(type: "INTEGER", nullable: false),
                    Brightness = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KeyBindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyCode = table.Column<int>(type: "INTEGER", nullable: false),
                    Alt = table.Column<bool>(type: "INTEGER", nullable: false),
                    Shift = table.Column<bool>(type: "INTEGER", nullable: false),
                    Control = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyBindings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ColorConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplyColor = table.Column<bool>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ColorConfigurations_Configurations_Id",
                        column: x => x.Id,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemperatureConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplyIntensity = table.Column<bool>(type: "INTEGER", nullable: false),
                    Intensity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemperatureConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemperatureConfigurations_Configurations_Id",
                        column: x => x.Id,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplyConfigurationCommands");

            migrationBuilder.DropTable(
                name: "ColorConfigurations");

            migrationBuilder.DropTable(
                name: "TemperatureConfigurations");

            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "KeyBindings");
        }
    }
}
