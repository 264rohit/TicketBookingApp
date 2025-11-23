using TicketBookingApp.Models;

namespace TicketBookingApp.Dto
{
    public class CreateBookingDto
    {
        public string Name { get; set; }
        public int NumberOfTickets { get; set; }
        public TicketType TicketType { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
