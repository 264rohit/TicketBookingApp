using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketBookingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExtraPerson",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraPerson",
                table: "Bookings");
        }
    }
}
