using sos100_bibliotek.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Lägg till Controllers och Views
builder.Services.AddControllersWithViews();

// 2. Session-inställningar (Räcker med en gång!)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Registrera alla HttpClients
// NotificationsAPI
builder.Services.AddHttpClient("NotificationsAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5235/");
});
builder.Services.AddScoped<NotificationService>();

// CatalogueService
builder.Services.AddHttpClient<CatalogueService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5149");
});

// UserApiService (Här pratar vi med din klasskamrats API)
var userApiUrl = builder.Configuration["ApiSettings:UserApiBaseUrl"] ?? "http://localhost:5027";

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient<UserApiService>(client =>
    {
        client.BaseAddress = new Uri(userApiUrl);
    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Tillåter lokala certifikat under utveckling
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}
else
{
    builder.Services.AddHttpClient<UserApiService>(client =>
    {
        client.BaseAddress = new Uri(userApiUrl);
    });
}

var app = builder.Build();

// 4. Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Viktigt för CSS/JS

app.UseRouting();

// VIKTIGT: UseSession måste ligga EFTER UseRouting men FÖRE UseAuthorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();