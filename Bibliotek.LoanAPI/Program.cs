using Microsoft.EntityFrameworkCore;
using Bibliotek.LoanAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrera databasen
builder.Services.AddDbContext<LoanDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Registrera CORS (Gör detta INNAN builder.Build)
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

// 3. Registrera Controllers
builder.Services.AddControllers();

// 4. Registrera HttpClient
builder.Services.AddHttpClient();

// 5. OpenAPI (Swagger)
builder.Services.AddOpenApi();

var app = builder.Build(); // Här låses konfigurationen

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Aktivera CORS-policyn vi skapade ovan
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();