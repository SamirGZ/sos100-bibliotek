using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Data;
using Scalar.AspNetCore; // Se till att denna using finns överst!

var builder = WebApplication.CreateBuilder(args);

// --- HÄR REGISTRERAR JAG ALLA TJÄNSTER ---

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

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddOpenApi(); // Denna genererar JSON-dokumentationen

var app = builder.Build(); 

// --- HÄR STÄLLER JAG IN HUR APPEN SKA KÖRAS ---

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // DENNA RAD SAKNADES: Den mappar upp det grafiska gränssnittet
    app.MapScalarApiReference(); 
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();