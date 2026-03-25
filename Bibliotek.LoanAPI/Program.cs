using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. DATABAS (SQLite)
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// 3. HTTP CLIENTS
builder.Services.AddHttpClient(); // Standardklient för generella anrop

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build(); 

// --- NYTT: SKAPA DATABASEN AUTOMATISKT I AZURE ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LoanDbContext>();
        // Denna rad ser till att LoanDatabase.db skapas och att tabellerna finns
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ett fel uppstod när databasen skulle skapas.");
    }
}
// ------------------------------------------------

// 4. PIPELINE
app.MapOpenApi();
app.MapScalarApiReference(); 

app.UseCors("AllowAll");
// Vi tar bort HttpsRedirection temporärt för att eliminera certifikat-strul i Azure
// app.UseHttpsRedirection(); 

app.UseAuthorization();
app.MapControllers();

app.Run();