using sos100_bibliotek.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRERA TJÄNSTER (SERVICES) - Allt här inne läggs i "verktygslådan"
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// FIX: Denna rad flyttades upp hit (ovanför builder.Build)
builder.Services.AddHttpClient(); 

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

builder.Services.AddScoped<NotificationService>();

// 2. BYGG APPLIKATIONEN - Här låses listan över tjänster
var app = builder.Build();

// 3. KONFIGURERA PIPELINE (MIDDLEWARE) - Hur anrop hanteras
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session måste ligga före Authorization
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 4. KÖR!
app.Run();