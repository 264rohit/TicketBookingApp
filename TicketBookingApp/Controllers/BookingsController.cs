using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using TicketBookingApp.Data;
using TicketBookingApp.Dto;
using TicketBookingApp.Models;
using TicketBookingApp.Services;
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

        private string GetEmailFromToken()
        {
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader))
                return null;

            try
            {
                var token = authHeader.Replace("Bearer ", "");
                var email = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                return AuthorizationService.IsAuthorized(email) ? email : null;
            }
            catch
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (string.IsNullOrEmpty(GetEmailFromToken()))
                return Unauthorized(new { message = "Unauthorized" });

            var bookings = await _context.Bookings.OrderByDescending(b => b.BookingDate).ToListAsync();
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (string.IsNullOrEmpty(GetEmailFromToken()))
                return Unauthorized(new { message = "Unauthorized" });

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();
            return Ok(booking);
        }

        [HttpGet("by-number/{bookingNumber}")]
        public async Task<IActionResult> GetByBookingNumber(string bookingNumber)
        {
            if (string.IsNullOrEmpty(GetEmailFromToken()))
                return Unauthorized(new { message = "Unauthorized" });

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);

            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            return Ok(booking);
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel()
        {
            if (string.IsNullOrEmpty(GetEmailFromToken()))
                return Unauthorized(new { message = "Unauthorized" });

            var bookings = await _context.Bookings
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Bookings");

            // Header row
            worksheet.Cell(1, 1).Value = "Name";
            worksheet.Cell(1, 2).Value = "PhoneNumber";
            worksheet.Cell(1, 3).Value = "NumberOfTickets";
            worksheet.Cell(1, 4).Value = "TicketType";
            worksheet.Cell(1, 5).Value = "BookingDate";
            worksheet.Cell(1, 6).Value = "BookingNumber";
            worksheet.Cell(1, 7).Value = "ExtraPerson";

            // Data rows
            var row = 2;
            foreach (var b in bookings)
            {
                worksheet.Cell(row, 1).Value = b.Name;
                worksheet.Cell(row, 2).Value = b.PhoneNumber;
                worksheet.Cell(row, 3).Value = b.NumberOfTickets;
                worksheet.Cell(row, 4).Value = b.TicketType.ToString();
                worksheet.Cell(row, 5).Value = b.BookingDate;
                worksheet.Cell(row, 6).Value = b.BookingNumber;
                worksheet.Cell(row, 7).Value = b.ExtraPerson;
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

        private string PrefixFor(TicketType type)
        {
            return type switch
            {
                TicketType.StandardStag => "SS",
                TicketType.PremiumPlatinum => "PP",
                TicketType.TitaniumTable => "5TT",
                TicketType.TitaniumTable10 => "10TT",
                TicketType.PremiumTitaniumTable => "10PTT",
                _ => "XX",
            };
        }

        private string GenerateUniqueCode(int digits = 4)
        {
            var rng = new Random();
            var max = (int)Math.Pow(10, digits) - 1;
            var min = (int)Math.Pow(10, digits - 1);
            return rng.Next(min, max + 1).ToString().PadLeft(digits, '0');
        }

        [HttpPost]
        public async Task<ActionResult<Booking>> CreateBooking([FromBody] CreateBookingDto dto)
        {
            if (string.IsNullOrEmpty(GetEmailFromToken()))
                return Unauthorized(new { message = "Unauthorized" });

            if (dto == null)
                return BadRequest(new { message = "Request body is required" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                string bookingNumber;
                var attempts = 0;

                do
                {
                    var prefix = PrefixFor(dto.TicketType);
                    bookingNumber = prefix + GenerateUniqueCode(4);
                    attempts++;

                    // safety: after many attempts fall back to a deterministic short GUID fragment
                    if (attempts > 10)
                    {
                        bookingNumber = PrefixFor(dto.TicketType) + Guid.NewGuid().ToString("N").Substring(0, 4);
                        break;
                    }
                }
                while (await _context.Bookings.AnyAsync(b => b.BookingNumber == bookingNumber));

                var booking = new Booking
                {
                    Name = dto.Name,
                    NumberOfTickets = dto.NumberOfTickets,
                    TicketType = dto.TicketType,
                    BookingDate = DateTime.Now,
                    BookingNumber = bookingNumber,
                    PhoneNumber = dto.PhoneNumber,
                    ExtraPerson = dto.ExtraPerson
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
            }
            catch (DbUpdateException dbEx)
            {
                // log the error server-side
                Console.WriteLine("Database error creating booking: " + dbEx.Message);
                return StatusCode(500, new { message = "Database error while creating booking." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating booking: " + ex.Message);
                return StatusCode(500, new { message = "An unexpected error occurred while creating booking." });
            }
        }

        [HttpDelete("by-number/{bookingNumber}")]
        public async Task<IActionResult> DeleteByBookingNumber(string bookingNumber)
        {
            if (string.IsNullOrEmpty(GetEmailFromToken()))
                return Unauthorized(new { message = "Unauthorized" });

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
