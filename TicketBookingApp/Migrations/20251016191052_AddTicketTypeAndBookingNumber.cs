using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketBookingApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketTypeAndBookingNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookingNumber",
                table: "Bookings",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TicketType",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingNumber",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TicketType",
                table: "Bookings");
        }
    }
}
