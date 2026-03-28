using Microsoft.EntityFrameworkCore;
using NotificationsService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddOpenApi();

static string ResolveSqliteConnectionString(IConfiguration config)
{
    var configured = config.GetConnectionString("DefaultConnection");
    return string.IsNullOrWhiteSpace(configured) ? "Data Source=notifications.db" : configured;
}

static string ResolveSqliteDbPath(string connectionString)
{
    const string prefix = "Data Source=";
    if (!connectionString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
    {
        return connectionString;
    }

    var dbPart = connectionString.Substring(prefix.Length).Trim().Trim('"');
    if (Path.IsPathRooted(dbPart))
    {
        return connectionString;
    }

    // Azure App Service: HOME is the persistent location when enabled.
    var isAzure = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
    if (!isAzure)
    {
        return connectionString;
    }

    var home = Environment.GetEnvironmentVariable("HOME");
    if (string.IsNullOrWhiteSpace(home))
    {
        return connectionString;
    }

    var dataDir = Path.Combine(home, "data");
    Directory.CreateDirectory(dataDir);
    var dbPath = Path.Combine(dataDir, dbPart);
    return $"{prefix}{dbPath}";
}

var sqliteConn = ResolveSqliteDbPath(ResolveSqliteConnectionString(builder.Configuration));
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(sqliteConn));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", b =>
        b.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Initiera/migrera databasen vid start (men låt appen starta även om DB strular)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        logger.LogInformation("Notifications DB datasource: {DataSource}", context.Database.GetDbConnection().DataSource);
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed on startup.");
    }
}

app.MapGet("/", () => Results.Ok("NotificationsService running"));
app.MapOpenApi();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();