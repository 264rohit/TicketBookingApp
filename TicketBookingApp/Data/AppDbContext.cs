using Microsoft.EntityFrameworkCore;
using TicketBookingApp.Models;

namespace TicketBookingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Booking> Bookings { get; set; }
    }
}
