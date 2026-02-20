using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OgrenciBilgiSistemi.Abstractions;
using OgrenciBilgiSistemi.Data;
using OgrenciBilgiSistemi.Hubs;
using OgrenciBilgiSistemi.Models;
using OgrenciBilgiSistemi.Services;
using OgrenciBilgiSistemi.Services.Implementations;
using OgrenciBilgiSistemi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 🔗 Veritabanı bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Veritabanı bağlantı dizesi (DefaultConnection) bulunamadı.");
}
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 💼 Servislerin eklenmesi
builder.Services.AddControllersWithViews();
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
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;                
        options.Cookie.IsEssential = true;
    });

// 🛡️ Rol Bazlı Yetkilendirme
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// 🌱 Admin kullanıcı seed işlemi (şifre hashleme)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = new PasswordHasher<KullaniciModel>();

    var admin = context.Kullanicilar.FirstOrDefault(k => k.KullaniciAdi == "admin");

    if (admin != null)
    {
        if (string.IsNullOrWhiteSpace(admin.Sifre) || !admin.Sifre.StartsWith("AQ"))
        {
            admin.Sifre = passwordHasher.HashPassword(admin, "admin123");
            context.SaveChanges();
        }
    }
    else
    {
        var yeniAdmin = new KullaniciModel
        {
            KullaniciAdi = "admin",
            AdminMi = true,
            KullaniciDurum = true,
            BeniHatirla = false,
            BirimId = null
        };
        yeniAdmin.Sifre = passwordHasher.HashPassword(yeniAdmin, "admin123");
        context.Kullanicilar.Add(yeniAdmin);
        context.SaveChanges();
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
