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
        [MaxLength(6)]
        public string BookingNumber { get; set; }
    }
}
