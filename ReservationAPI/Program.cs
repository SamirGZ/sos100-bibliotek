using Microsoft.EntityFrameworkCore;
using ReservationApi.Data;
using ReservationApi.Options;
using ReservationApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.Configure<ServiceUrlsOptions>(
    builder.Configuration.GetSection(ServiceUrlsOptions.SectionName));

builder.Services.AddHttpClient(nameof(ReservationUpstreamClient));
builder.Services.AddScoped<IReservationUpstreamClient, ReservationUpstreamClient>();

builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseSqlite("Data Source=reservations.db"));

builder.Services.AddOpenApi();

var app = builder.Build();

// Detta tvingar Azure att skapa SQLite-filen och tabellerna om de inte redan finns.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ReservationDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();