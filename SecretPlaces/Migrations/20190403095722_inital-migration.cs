using Microsoft.EntityFrameworkCore.Migrations;

namespace WorldOfTravels.Migrations
{
    public partial class initalmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Continent",
                table: "Restaurant");

            migrationBuilder.AddColumn<int>(
                name: "RestaurantType",
                table: "Restaurant",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestaurantType",
                table: "Restaurant");

            migrationBuilder.AddColumn<int>(
                name: "Continent",
                table: "Restaurant",
                nullable: false,
                defaultValue: 0);
        }
    }
}
