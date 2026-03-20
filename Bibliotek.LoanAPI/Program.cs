using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Data;
using Bibliotek.LoanAPI.Models; // Se till att denna finns för User-klassen
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. REGISTRERA TJÄNSTER ---

builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddHttpClient();
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

// --- 3. SEED DATA (Lösningen på Foreign Key Error 19) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<LoanDbContext>();
    
    // Skapar databas-tabellerna om de inte finns
    context.Database.EnsureCreated();

    // Kontrollera om User 1 saknas
    if (!context.Users.Any(u => u.Id == 1))
    {
        context.Users.Add(new User 
        { 
            Id = 1, 
            Username = "Shahin", 
            Email = "demo@bibliotek.se" 
        });
        context.SaveChanges();
        Console.WriteLine(">>> SEED: Demo-användare med Id 1 har skapats!");
    }
}

app.Run();