using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using TicketBookingApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Enable CORS (for React frontend)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin()); // <-- changed from AllowCredentials
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

var app = builder.Build();

// Ensure database migrations are applied at startup so new columns (e.g. PhoneNumber) exist
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Apply any pending migrations
        db.Database.Migrate();

        // As a safety fallback: if the PhoneNumber column doesn't exist, add it
        var conn = db.Database.GetDbConnection();
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Bookings' AND COLUMN_NAME = 'PhoneNumber'";
            var exists = (int)cmd.ExecuteScalar() > 0;
            if (!exists)
            {
                cmd.CommandText = @"ALTER TABLE [Bookings] ADD [PhoneNumber] nvarchar(20) NULL";
                cmd.ExecuteNonQuery();
            }
        }
        conn.Close();
    }
    catch (Exception ex)
    {
        // Log but don't crash the app on startup migration issues
        Console.WriteLine("Error while applying migrations or updating schema: " + ex.Message);
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
