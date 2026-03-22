using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Data;
using Bibliotek.LoanAPI.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. REGISTRERA TJÄNSTER ---
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// REGISTRERA HTTPCLIENT FÖR USER-API
builder.Services.AddHttpClient("UserClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5027"); // Säkerställ att porten stämmer!
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddHttpClient(); // Standard klient för notifikationer
builder.Services.AddOpenApi(); 

var app = builder.Build(); 

// --- 2. KONFIGURERA PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); 
}

app.UseCors("AllowAll"); 
app.UseAuthorization();
app.MapControllers();

// --- 3. SEED DATA ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<LoanDbContext>();
    context.Database.EnsureCreated();

    if (!context.Users.Any(u => u.Id == 1))
    {
        context.Users.Add(new User { Id = 1, Username = "Shahin", Email = "demo@bibliotek.se" });
        context.SaveChanges();
        Console.WriteLine(">>> SEED: Demo-användare skapad!");
    }
}

app.Run();