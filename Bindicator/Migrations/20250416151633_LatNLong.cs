using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bindicator.Migrations
{
    /// <inheritdoc />
    public partial class LatNLong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "SensorReadings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "SensorReadings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "SensorReadings");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "SensorReadings");
        }
    }
}
