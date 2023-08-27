using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScreenTemperature.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KeyBindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Alt = table.Column<bool>(type: "INTEGER", nullable: false),
                    Shift = table.Column<bool>(type: "INTEGER", nullable: false),
                    Control = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyBindings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DecreaseBrightnessByActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyBindingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecreaseBrightnessByActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecreaseBrightnessByActions_KeyBindings_KeyBindingId",
                        column: x => x.KeyBindingId,
                        principalTable: "KeyBindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncreaseBrightnessByActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyBindingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncreaseBrightnessByActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncreaseBrightnessByActions_KeyBindings_KeyBindingId",
                        column: x => x.KeyBindingId,
                        principalTable: "KeyBindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetBrightnessToActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyBindingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetBrightnessToActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetBrightnessToActions_KeyBindings_KeyBindingId",
                        column: x => x.KeyBindingId,
                        principalTable: "KeyBindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplyConfigurationActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyBindingId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplyConfigurationActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplyConfigurationActions_KeyBindings_KeyBindingId",
                        column: x => x.KeyBindingId,
                        principalTable: "KeyBindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplyConfigurationActions_Profiles_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ColorConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DevicePath = table.Column<string>(type: "TEXT", nullable: false),
                    Brightness = table.Column<byte>(type: "INTEGER", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ColorConfigurations_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemperatureConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DevicePath = table.Column<string>(type: "TEXT", nullable: false),
                    Brightness = table.Column<byte>(type: "INTEGER", nullable: false),
                    Intensity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemperatureConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemperatureConfigurations_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplyConfigurationActions_ConfigurationId",
                table: "ApplyConfigurationActions",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplyConfigurationActions_KeyBindingId",
                table: "ApplyConfigurationActions",
                column: "KeyBindingId");

            migrationBuilder.CreateIndex(
                name: "IX_ColorConfigurations_ProfileId",
                table: "ColorConfigurations",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DecreaseBrightnessByActions_KeyBindingId",
                table: "DecreaseBrightnessByActions",
                column: "KeyBindingId");

            migrationBuilder.CreateIndex(
                name: "IX_IncreaseBrightnessByActions_KeyBindingId",
                table: "IncreaseBrightnessByActions",
                column: "KeyBindingId");

            migrationBuilder.CreateIndex(
                name: "IX_SetBrightnessToActions_KeyBindingId",
                table: "SetBrightnessToActions",
                column: "KeyBindingId");

            migrationBuilder.CreateIndex(
                name: "IX_TemperatureConfigurations_ProfileId",
                table: "TemperatureConfigurations",
                column: "ProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplyConfigurationActions");

            migrationBuilder.DropTable(
                name: "ColorConfigurations");

            migrationBuilder.DropTable(
                name: "DecreaseBrightnessByActions");

            migrationBuilder.DropTable(
                name: "IncreaseBrightnessByActions");

            migrationBuilder.DropTable(
                name: "SetBrightnessToActions");

            migrationBuilder.DropTable(
                name: "TemperatureConfigurations");

            migrationBuilder.DropTable(
                name: "KeyBindings");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
