using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// --- HÄR REGISTRERAR JAG ALLA TJÄNSTER ---

// Kopplar upp databasen (sqlite)
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Fixar CORS så att React-appen på localhost får hämta data utan att bli blockad
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
builder.Services.AddOpenApi();

var app = builder.Build(); 

// --- HÄR STÄLLER JAG IN HUR APPEN SKA KÖRAS ---

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Aktiverar CORS-inställningen jag gjorde ovanför
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();