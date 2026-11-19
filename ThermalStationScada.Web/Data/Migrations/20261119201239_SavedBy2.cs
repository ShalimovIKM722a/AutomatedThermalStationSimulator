using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThermalStationScada.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SavedBy2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SavedBy",
                table: "SensorSamples",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SavedBy",
                table: "SensorSamples");
        }
    }
}
