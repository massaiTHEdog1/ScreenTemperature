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
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
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
                name: "KeyBindingActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KeyBindingId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyBindingActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KeyBindingActions_KeyBindings_KeyBindingId",
                        column: x => x.KeyBindingId,
                        principalTable: "KeyBindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationProfile",
                columns: table => new
                {
                    ConfigurationsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProfilesId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationProfile", x => new { x.ConfigurationsId, x.ProfilesId });
                    table.ForeignKey(
                        name: "FK_ConfigurationProfile_Configurations_ConfigurationsId",
                        column: x => x.ConfigurationsId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfigurationProfile_Profiles_ProfilesId",
                        column: x => x.ProfilesId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplyProfileActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplyProfileActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplyProfileActions_KeyBindingActions_Id",
                        column: x => x.Id,
                        principalTable: "KeyBindingActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplyProfileActions_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplyProfileActions_ProfileId",
                table: "ApplyProfileActions",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigurationProfile_ProfilesId",
                table: "ConfigurationProfile",
                column: "ProfilesId");

            migrationBuilder.CreateIndex(
                name: "IX_KeyBindingActions_KeyBindingId",
                table: "KeyBindingActions",
                column: "KeyBindingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplyProfileActions");

            migrationBuilder.DropTable(
                name: "ColorConfigurations");

            migrationBuilder.DropTable(
                name: "ConfigurationProfile");

            migrationBuilder.DropTable(
                name: "TemperatureConfigurations");

            migrationBuilder.DropTable(
                name: "KeyBindingActions");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "KeyBindings");
        }
    }
}
