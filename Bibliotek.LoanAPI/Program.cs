using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrera databasen
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Registrera Controllers
builder.Services.AddControllers();

// 3. Registrera HttpClient (Viktigt för att prata med klasskamratens API!)
builder.Services.AddHttpClient();

// 4. OpenAPI (Swagger)
builder.Services.AddOpenApi();

var app = builder.Build();

// Allt efter denna rad är konfiguration av hur appen körs

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();