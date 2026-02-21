using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemi.Abstractions;
using OgrenciBilgiSistemi.Data;
using OgrenciBilgiSistemi.Hubs;
using OgrenciBilgiSistemi.Services;
using OgrenciBilgiSistemi.Services.Implementations;
using OgrenciBilgiSistemi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var bootstrapSection = builder.Configuration.GetSection("Bootstrap");
var environment = builder.Environment;

// 🔗 Veritabanı bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Veritabanı bağlantı dizesi (DefaultConnection) bulunamadı.");
}

if (environment.IsProduction() && connectionString.Contains("DESKTOP-", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "Production ortamında makineye bağımlı bağlantı dizesi tespit edildi. ConnectionStrings__DefaultConnection değerini environment/secrets ile verin.");
}

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptions =>
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// 💼 Servislerin eklenmesi
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});
builder.Services.AddScoped<IAidatService, AidatService>();
builder.Services.AddScoped<IGecisService, GecisService>();
builder.Services.AddSingleton<IZKTecoService, ZKTecoService>();
builder.Services.AddScoped<IKartOkuService, KartOkuService>();
builder.Services.AddScoped<IYemekhaneService, YemekhaneService>();
builder.Services.AddScoped<ICihazService, CihazService>();
builder.Services.AddScoped<IOgrenciService, OgrenciService>();
builder.Services.AddScoped<IOgrenciVeliService, OgrenciVeliService>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
builder.Services.AddScoped<IBirimService, BirimService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IPersonelService, PersonelService>();
builder.Services.AddScoped<IZiyaretciService, ZiyaretciService>();
builder.Services.AddScoped<IKitapService, KitapService>();
builder.Services.AddScoped<IKitapDetayService, KitapDetayService>();

builder.Services.AddHostedService<CardReadEventHandlerService>();
builder.Services.AddHostedService<ZkConnectionMonitorHostedService>();


// 🔔 SignalR yapılandırması
builder.Services.AddSignalR();

// 🧠 Bellek önbelleği
builder.Services.AddMemoryCache();

// 🔐 Kimlik Doğrulama (Cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Hesaplar/Giris";
        options.AccessDeniedPath = "/Hesaplar/YetkisizGiris";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.IsEssential = true;
    });

// 🛡️ Rol Bazlı Yetkilendirme
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// 🌱 Veritabanı migration işlemi
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    try
    {
        // Bağlantıyı doğrula (login/connection hatalarını hızlı yakalar)
        context.Database.OpenConnection();
        context.Database.CloseConnection();

        // Migration: default sadece Development'ta çalışsın (Prod'da otomatik migration riskini azaltır)
        // İstersen appsettings'ten BootstrapAdmin:ApplyMigrations=true diyerek açabilirsin.
        var applyMigrations =
            bootstrapSection.GetValue<bool?>("ApplyMigrations")
            ?? app.Environment.IsDevelopment();

        if (applyMigrations)
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex,
            "Veritabanı bağlantısı kurulamadı veya migration uygulanamadı. ConnectionStrings__DefaultConnection değerini, veritabanı adını ve kullanıcı yetkilerini kontrol edin.");
        throw;
    }
}

// 🌐 Hata yönetimi
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔐 Kimlik doğrulama/Yetkilendirme middleware'leri
app.UseAuthentication();
app.UseAuthorization();

// 🧭 Varsayılan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Hesaplar}/{action=Giris}/{id?}");

// 📡 SignalR Hub
app.MapHub<KartOkuHub>("/kartOkuHub");

// ✅ Keep-alive: yetkili kullanıcıya açık, auth sonrası
app.MapGet("/keep-alive", () => Results.Ok()).RequireAuthorization();

app.Run();
