using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemi.Abstractions;
using OgrenciBilgiSistemi.Data;
using OgrenciBilgiSistemi.Hubs;
using OgrenciBilgiSistemi.Models;
using OgrenciBilgiSistemi.Services;
using OgrenciBilgiSistemi.Services.Implementations;
using OgrenciBilgiSistemi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
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
    options.UseSqlServer(connectionString));

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

// 🌱 Bootstrap admin kullanıcı seed işlemi (güvenli kurulum)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("BootstrapAdmin");
    var passwordHasher = new PasswordHasher<KullaniciModel>();
    var bootstrapSection = builder.Configuration.GetSection("BootstrapAdmin");

    var bootstrapEnabled = bootstrapSection.GetValue<bool?>("Enabled") ?? false;
    var bootstrapUsername = bootstrapSection["Username"];
    var bootstrapPassword = bootstrapSection["Password"];

    if (app.Environment.IsProduction() && bootstrapEnabled &&
        (string.IsNullOrWhiteSpace(bootstrapUsername) || string.IsNullOrWhiteSpace(bootstrapPassword)))
    {
        throw new InvalidOperationException(
            "Production ortamında BootstrapAdmin etkinse BootstrapAdmin:Username ve BootstrapAdmin:Password zorunludur.");
    }

    if (!bootstrapEnabled)
    {
        logger.LogInformation("Bootstrap admin oluşturma devre dışı bırakıldı (BootstrapAdmin:Enabled=false).");
    }
    else if (string.IsNullOrWhiteSpace(bootstrapUsername) || string.IsNullOrWhiteSpace(bootstrapPassword))
    {
        logger.LogWarning(
            "Bootstrap admin atlandı. BootstrapAdmin:Username ve BootstrapAdmin:Password ayarlanmalı (tercihen environment variable/secrets ile).");
    }
    else
    {
        var admin = context.Kullanicilar.FirstOrDefault(k => k.KullaniciAdi == bootstrapUsername);

        if (admin != null)
        {
            if (string.IsNullOrWhiteSpace(admin.Sifre) || !admin.Sifre.StartsWith("AQ"))
            {
                admin.Sifre = passwordHasher.HashPassword(admin, bootstrapPassword);
                context.SaveChanges();
                logger.LogInformation("Mevcut bootstrap admin şifresi güvenli hash ile güncellendi.");
            }
        }
        else
        {
            var yeniAdmin = new KullaniciModel
            {
                KullaniciAdi = bootstrapUsername,
                AdminMi = true,
                KullaniciDurum = true,
                BeniHatirla = false,
                BirimId = null
            };
            yeniAdmin.Sifre = passwordHasher.HashPassword(yeniAdmin, bootstrapPassword);
            context.Kullanicilar.Add(yeniAdmin);
            context.SaveChanges();
            logger.LogInformation("Bootstrap admin kullanıcısı oluşturuldu: {Username}", bootstrapUsername);
        }
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
