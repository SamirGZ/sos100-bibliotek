using sos100_bibliotek.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Konfiguration av sessionstillstånd
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- REGISTRERING AV HTTP-KLIENTER OCH TJÄNSTER ---

// User API
builder.Services.AddHttpClient<UserApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:UserApiBaseUrl"] ?? "http://localhost:5027");
});

// Loan API
builder.Services.AddHttpClient("LoanAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5029/");
});

// Catalogue Service
builder.Services.AddHttpClient<CatalogueService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5149");
});

// Registrera namngiven klient för Notifikationer
builder.Services.AddHttpClient("NotificationsAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5235/");
});

// Registrera tjänsten så att NotificationController kan hitta den
builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Aktivering av session och auktorisering i korrekt ordning
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();