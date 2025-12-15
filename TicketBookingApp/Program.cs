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
            // Ensure PhoneNumber exists
            cmd.CommandText = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Bookings' AND COLUMN_NAME = 'PhoneNumber'";
            var existsPhone = (int)cmd.ExecuteScalar() > 0;
            if (!existsPhone)
            {
                cmd.CommandText = @"ALTER TABLE [Bookings] ADD [PhoneNumber] nvarchar(20) NULL";
                cmd.ExecuteNonQuery();
            }

            // Ensure BookingNumber length is sufficient (at least 12). If shorter, alter the column.
            cmd.CommandText = @"SELECT CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Bookings' AND COLUMN_NAME = 'BookingNumber'";
            var obj = cmd.ExecuteScalar();
            if (obj != null && obj != DBNull.Value)
            {
                if (int.TryParse(obj.ToString(), out var currentLength))
                {
                    if (currentLength < 12)
                    {
                        // Alter the column to nvarchar(12) NOT NULL (BookingNumber is required in the model)
                        try
                        {
                            cmd.CommandText = @"ALTER TABLE [Bookings] ALTER COLUMN [BookingNumber] nvarchar(12) NOT NULL";
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception alterEx)
                        {
                            // If altering to NOT NULL fails (rare), try altering to nullable then back to not null
                            Console.WriteLine("Failed to alter BookingNumber to NOT NULL directly: " + alterEx.Message);
                            try
                            {
                                cmd.CommandText = @"ALTER TABLE [Bookings] ALTER COLUMN [BookingNumber] nvarchar(12) NULL";
                                cmd.ExecuteNonQuery();
                                cmd.CommandText = @"UPDATE [Bookings] SET [BookingNumber] = ISNULL([BookingNumber], '') WHERE [BookingNumber] IS NULL";
                                cmd.ExecuteNonQuery();
                                cmd.CommandText = @"ALTER TABLE [Bookings] ALTER COLUMN [BookingNumber] nvarchar(12) NOT NULL";
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine("Failed to safely alter BookingNumber column: " + innerEx.Message);
                            }
                        }
                    }
                }
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
