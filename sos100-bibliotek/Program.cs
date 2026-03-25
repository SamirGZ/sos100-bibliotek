using sos100_bibliotek.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRERA TJÄNSTER
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- HTTP CLIENTS FÖR AZURE ---

// 1. Catalogue Service
builder.Services.AddHttpClient<CatalogueService>(client =>
{
    var url = builder.Configuration["ServiceUrls:CatalogueApi"] ?? "http://localhost:5149";
    client.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
});

// 2. User API (Denna saknades!)
builder.Services.AddHttpClient<UserApiService>(client =>
{
    var url = builder.Configuration["ServiceUrls:UserApi"] ?? "http://localhost:5027";
    client.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
});

// 3. Notifications API (Namngiven klient)
builder.Services.AddHttpClient("NotificationsAPI", client => {
    var url = builder.Configuration["ServiceUrls:NotificationsApi"] ?? "http://localhost:5235/";
    client.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
});

// 4. Loan API (Om du har en LoanService)
builder.Services.AddHttpClient<LoanService>(client =>
{
    var url = builder.Configuration["ServiceUrls:LoanApi"] ?? "http://localhost:5029";
    client.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
});

// Registrera själva service-klasserna
builder.Services.AddScoped<NotificationService>();
// UserApiService och CatalogueService registreras automatiskt via AddHttpClient<T> ovan!

// ------------------------------

var app = builder.Build();

// 2. MIDDLEWARE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // Viktigt för inloggning!
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();