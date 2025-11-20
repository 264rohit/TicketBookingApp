using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using TicketBookingApp.Data;
using TicketBookingApp.Dto;
using TicketBookingApp.Models;
using ClosedXML.Excel;

namespace TicketBookingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bookings = await _context.Bookings.OrderByDescending(b => b.BookingDate).ToListAsync();
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();
            return Ok(booking);
        }

        [HttpGet("by-number/{bookingNumber}")]
        public async Task<IActionResult> GetByBookingNumber(string bookingNumber)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);

            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            return Ok(booking);
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var bookings = await _context.Bookings
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Bookings");

            // Header row
            worksheet.Cell(1, 1).Value = "Id";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "NumberOfTickets";
            worksheet.Cell(1, 4).Value = "TicketType";
            worksheet.Cell(1, 5).Value = "BookingDate";
            worksheet.Cell(1, 6).Value = "BookingNumber";

            // Data rows
            var row = 2;
            foreach (var b in bookings)
            {
                worksheet.Cell(row, 1).Value = b.Id.ToString();
                worksheet.Cell(row, 2).Value = b.Name;
                worksheet.Cell(row, 3).Value = b.NumberOfTickets;
                worksheet.Cell(row, 4).Value = b.TicketType.ToString();
                worksheet.Cell(row, 5).Value = b.BookingDate;
                worksheet.Cell(row, 6).Value = b.BookingNumber;
                row++;
            }

            // Format
            worksheet.Columns().AdjustToContents();
            worksheet.Column(5).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"bookings_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(stream.ToArray(), contentType, fileName);
        }

        [HttpPost]
        public async Task<ActionResult<Booking>> CreateBooking([FromBody] CreateBookingDto dto)
        {
            string bookingNumber;
            do
            {
                bookingNumber = new Random().Next(100000, 999999).ToString();
            }
            while (await _context.Bookings.AnyAsync(b => b.BookingNumber == bookingNumber));

            var booking = new Booking
            {
                Name = dto.Name,
                NumberOfTickets = dto.NumberOfTickets,
                TicketType = dto.TicketType,
                BookingDate = DateTime.Now,
                BookingNumber = bookingNumber
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }

        [HttpDelete("by-number/{bookingNumber}")]
        public async Task<IActionResult> DeleteByBookingNumber(string bookingNumber)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);

            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
