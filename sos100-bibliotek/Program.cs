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

builder.Services.AddHttpClient(); 

// --- HÄR ÄR ÄNDRINGARNA FÖR AZURE ---

// User API
builder.Services.AddHttpClient<UserApiService>(client =>
{
    // Vi läser från ServiceUrls:UserApi som du skapade i Azure
    var url = builder.Configuration["ServiceUrls:UserApi"] ?? "http://localhost:5027";
    client.BaseAddress = new Uri(url);
});

// Loan API
builder.Services.AddHttpClient("LoanAPI", client =>
{
    var url = builder.Configuration["ServiceUrls:LoanApi"] ?? "http://localhost:5029/";
    client.BaseAddress = new Uri(url);
});

// Catalogue Service
builder.Services.AddHttpClient<CatalogueService>(client =>
{
    var url = builder.Configuration["ServiceUrls:CatalogueApi"] ?? "http://localhost:5149";
    client.BaseAddress = new Uri(url);
});

// Notifications API
builder.Services.AddHttpClient("NotificationsAPI", client =>
{
    var url = builder.Configuration["ServiceUrls:NotificationsApi"] ?? "http://localhost:5235/";
    client.BaseAddress = new Uri(url);
});

// ------------------------------------

builder.Services.AddScoped<NotificationService>();

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
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();