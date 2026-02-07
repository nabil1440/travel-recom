using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClarifyAggregationDateAndLeaderboardSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "district_weather_snapshots",
                newName: "date");

            migrationBuilder.RenameIndex(
                name: "IX_district_weather_snapshots_DistrictId_Date",
                table: "district_weather_snapshots",
                newName: "IX_district_weather_snapshots_DistrictId_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "date",
                table: "district_weather_snapshots",
                newName: "Date");

            migrationBuilder.RenameIndex(
                name: "IX_district_weather_snapshots_DistrictId_date",
                table: "district_weather_snapshots",
                newName: "IX_district_weather_snapshots_DistrictId_Date");
        }
    }
}
