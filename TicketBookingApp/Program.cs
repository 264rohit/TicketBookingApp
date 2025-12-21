using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using TicketBookingApp.Data;
using TicketBookingApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

// Add DbContext - Use SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Enable CORS (for React frontend)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

var app = builder.Build();

// Ensure database migrations are applied at startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Apply any pending migrations
        db.Database.Migrate();

        // Seed previous data if tables are empty
        if (!db.Bookings.Any())
        {
            var seedData = new List<Booking>
            {
                new Booking { Id = Guid.Parse("16C2E31C-E23B-4C59-E191-08DE3BA1536D"), Name = "Kunal Sarak", NumberOfTickets = 1, BookingDate = new DateTime(2025, 12, 15, 11, 45, 20), BookingNumber = "5TT3532", TicketType = TicketType.TitaniumTable, PhoneNumber = "9529949266", ExtraPerson = 0 },
                new Booking { Id = Guid.Parse("CEA69B48-02F7-484C-FF79-08DE3FE6C466"), Name = "Varun Dalwani", NumberOfTickets = 1, BookingDate = new DateTime(2025, 12, 20, 22, 12, 30), BookingNumber = "PP6719", TicketType = TicketType.PremiumPlatinum, PhoneNumber = "7028274698", ExtraPerson = 0 },
                new Booking { Id = Guid.Parse("997C16EE-91BB-438B-FF7A-08DE3FE6C466"), Name = "Lavish duseja", NumberOfTickets = 1, BookingDate = new DateTime(2025, 12, 20, 22, 13, 2), BookingNumber = "PP6066", TicketType = TicketType.PremiumPlatinum, PhoneNumber = "7028274698", ExtraPerson = 0 },
                new Booking { Id = Guid.Parse("719A6824-4BD4-46EE-FF7B-08DE3FE6C466"), Name = "Aman Khursija", NumberOfTickets = 1, BookingDate = new DateTime(2025, 12, 20, 22, 13, 20), BookingNumber = "PP8528", TicketType = TicketType.PremiumPlatinum, PhoneNumber = "7028274698", ExtraPerson = 0 },
                new Booking { Id = Guid.Parse("38BC46CC-855E-4F30-FF7C-08DE3FE6C466"), Name = "Raj Lakhwani", NumberOfTickets = 1, BookingDate = new DateTime(2025, 12, 20, 22, 13, 43), BookingNumber = "PP3709", TicketType = TicketType.PremiumPlatinum, PhoneNumber = "7028274698", ExtraPerson = 0 },
                new Booking { Id = Guid.Parse("676C6C60-E54C-498C-FF7D-08DE3FE6C466"), Name = "Jayesh Thakare", NumberOfTickets = 1, BookingDate = new DateTime(2025, 12, 21, 17, 21, 31), BookingNumber = "PP6514", TicketType = TicketType.PremiumPlatinum, PhoneNumber = "8010802907", ExtraPerson = 0 },
                new Booking { Id = Guid.Parse("E3E3FD90-066A-44E4-FF7E-08DE3FE6C466"), Name = "Madhuri Thakare", NumberOfTickets = 1, BookingDate = new DateTime(2025, 12, 21, 17, 23, 46), BookingNumber = "PP2712", TicketType = TicketType.PremiumPlatinum, PhoneNumber = "8010802907", ExtraPerson = 0 },
                new Booking { Id = Guid.Parse("F330022A-4945-4EFD-FF7F-08DE3FE6C466"), Name = "Tanisha Patil", NumberOfTickets = 1, BookingDate = new DateTime(2025, 12, 21, 17, 25, 10), BookingNumber = "PP9591", TicketType = TicketType.PremiumPlatinum, PhoneNumber = "8010802907", ExtraPerson = 0 },
                new Booking { Id = Guid.Parse("962AAF88-34AB-4772-FF80-08DE3FE6C466"), Name = "Hitesh Wankhede", NumberOfTickets = 1, BookingDate = new DateTime(2025, 12, 21, 17, 25, 33), BookingNumber = "PP3892", TicketType = TicketType.PremiumPlatinum, PhoneNumber = "8010802907", ExtraPerson = 0 }
            };

            db.Bookings.AddRange(seedData);
            await db.SaveChangesAsync();
            Console.WriteLine($"Seeded {seedData.Count} booking records.");
        }

        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error while applying migrations or seeding data: " + ex.Message);
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve static files from wwwroot (frontend)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

app.UseAuthorization();
app.MapControllers();
app.Run();
