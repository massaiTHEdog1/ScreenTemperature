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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Label = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KeyBindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
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
                name: "ScreenConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConfigurationId = table.Column<int>(type: "INTEGER", nullable: false),
                    DevicePath = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScreenConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScreenConfiguration_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplyConfigurationActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyBindingId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfigurationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplyConfigurationActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplyConfigurationActions_Configurations_ConfigurationId",
                        column: x => x.ConfigurationId,
                        principalTable: "Configurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplyConfigurationActions_KeyBindings_KeyBindingId",
                        column: x => x.KeyBindingId,
                        principalTable: "KeyBindings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DecreaseBrightnessByActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyBindingId = table.Column<int>(type: "INTEGER", nullable: false),
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyBindingId = table.Column<int>(type: "INTEGER", nullable: false),
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyBindingId = table.Column<int>(type: "INTEGER", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_ApplyConfigurationActions_ConfigurationId",
                table: "ApplyConfigurationActions",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplyConfigurationActions_KeyBindingId",
                table: "ApplyConfigurationActions",
                column: "KeyBindingId");

            migrationBuilder.CreateIndex(
                name: "IX_DecreaseBrightnessByActions_KeyBindingId",
                table: "DecreaseBrightnessByActions",
                column: "KeyBindingId");

            migrationBuilder.CreateIndex(
                name: "IX_IncreaseBrightnessByActions_KeyBindingId",
                table: "IncreaseBrightnessByActions",
                column: "KeyBindingId");

            migrationBuilder.CreateIndex(
                name: "IX_ScreenConfiguration_ConfigurationId",
                table: "ScreenConfiguration",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_SetBrightnessToActions_KeyBindingId",
                table: "SetBrightnessToActions",
                column: "KeyBindingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplyConfigurationActions");

            migrationBuilder.DropTable(
                name: "DecreaseBrightnessByActions");

            migrationBuilder.DropTable(
                name: "IncreaseBrightnessByActions");

            migrationBuilder.DropTable(
                name: "ScreenConfiguration");

            migrationBuilder.DropTable(
                name: "SetBrightnessToActions");

            migrationBuilder.DropTable(
                name: "Configurations");

            migrationBuilder.DropTable(
                name: "KeyBindings");
        }
    }
}
