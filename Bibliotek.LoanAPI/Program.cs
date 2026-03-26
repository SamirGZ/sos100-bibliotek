using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Använder SQLite (smidigt i Azure så slipper vi en separat db-server)
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Tillåter alla anrop pga port-strul mellan frontend och backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Viktigt: AddHttpClient förhindrar port-utmattning mot externa API:er
builder.Services.AddHttpClient(); 

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build(); 

// --- Fix för molnet ---
// Skapar databasen automatiskt vid uppstart eftersom vi inte kan köra EF-migreringar manuellt i Azure
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LoanDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Kunde inte skapa databasen.");
    }
}

app.MapOpenApi();
app.MapScalarApiReference(); 

app.UseCors("AllowAll");

// Utkommenterad medvetet. Azures lastbalanserare strular ibland om vi tvingar HTTPS här.
// app.UseHttpsRedirection(); 

app.UseAuthorization();
app.MapControllers();

app.Run();