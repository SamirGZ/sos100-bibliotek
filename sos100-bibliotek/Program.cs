using sos100_bibliotek.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRERA TJÄNSTER
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

// VIKTIGT: Här kopplar vi CatalogueService till URL:en från Azure
builder.Services.AddHttpClient<CatalogueService>(client =>
{
    // Hämtar "ServiceUrls__CatalogueApi" från Azure Environment Variables
    var url = builder.Configuration["ServiceUrls:CatalogueApi"] 
              ?? "http://localhost:5149"; // Fallback för lokal körning
              
    client.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
});

// Registrera även dina andra tjänster på samma sätt...
builder.Services.AddHttpClient("NotificationsAPI", client => {
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:NotificationsApi"] ?? "http://localhost:5235/");
});

builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();