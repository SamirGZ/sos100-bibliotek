using sos100_bibliotek.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- HTTP CLIENTS FÖR AZURE ---

builder.Services.AddHttpClient<CatalogueService>(client =>
{
    var url = builder.Configuration["ServiceUrls:CatalogueApi"];
    client.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
});

builder.Services.AddHttpClient<UserApiService>(client =>
{
    var url = builder.Configuration["ServiceUrls:UserApi"];
    client.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
});

builder.Services.AddHttpClient("NotificationsAPI", client => {
    var url = builder.Configuration["ServiceUrls:NotificationsApi"];
    client.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
});

builder.Services.AddHttpClient<LoanService>(client =>
{
    var url = builder.Configuration["ServiceUrls:LoanApi"];
    client.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
});

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
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();