using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bindicator.Migrations
{
    /// <inheritdoc />
    public partial class Blank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "HighTemp",
                table: "EnvironmentReadings",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "LowTemp",
                table: "EnvironmentReadings",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighTemp",
                table: "EnvironmentReadings");

            migrationBuilder.DropColumn(
                name: "LowTemp",
                table: "EnvironmentReadings");
        }
    }
}
