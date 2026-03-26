using Microsoft.EntityFrameworkCore;
using NotificationsService.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. JSON-inställningar - Gör API:et extremt tillåtande
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null; 
    });

builder.Services.AddOpenApi();

// 2. ÄNDRING: Vi använder In-Memory istället för SQLite för att slippa filproblem i Azure
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("NotificationDb")); 

// 3. CORS - Öppna upp allt
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", b => 
        b.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// 4. Initiera databasen i minnet
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Skapar den virtuella databasen i RAM
        context.Database.EnsureCreated();
        Console.WriteLine(">>> DATABASE: Success - In-Memory DB is running.");
    }
    catch (Exception ex) 
    {
        Console.WriteLine($">>> DATABASE ERROR: {ex.Message}");
    }
}

// 5. Middleware-ordning
app.UseHttpsRedirection();

// KRITISKT: CORS måste ligga här för att Azure ska tillåta anrop från LoanAPI
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();