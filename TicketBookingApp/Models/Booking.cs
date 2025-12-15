using System;
using System.ComponentModel.DataAnnotations;

namespace TicketBookingApp.Models
{
    public class Booking
    {
        public Guid Id { get; set; }               // unique ID
        public string Name { get; set; }           // person name
        public int NumberOfTickets { get; set; }   // how many tickets
        public DateTime BookingDate { get; set; }  // date of booking

        [Required]
        public TicketType TicketType { get; set; }  // 👈 New Enum field

        [Required]
        [MaxLength(12)]
        public string BookingNumber { get; set; }

        // New phone number field
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public int ExtraPerson { get; set; } = 0;
    }
}
