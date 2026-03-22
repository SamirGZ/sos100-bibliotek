using sos100_bibliotek.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("NotificationsAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5235/");
});
builder.Services.AddScoped<NotificationService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient<UserApiService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:UserApiBaseUrl"]!);
    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}
else
{
    builder.Services.AddHttpClient<UserApiService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:UserApiBaseUrl"]!);
    });
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient<UserApiService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:UserApiBaseUrl"]!);
    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}
else
{
    builder.Services.AddHttpClient<UserApiService>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:UserApiBaseUrl"]!);
    });
}

// Add HTTPClient to CatalogueService
builder.Services.AddHttpClient<CatalogueService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5149/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// testar bara commit 
app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();