# OgrenciBilgiSistemiApp — Kapsamlı Analiz Raporu

Hazırlanma tarihi: 2026-02-22
Analiz edilen branch: `claude/analyze-app-issues-qAmXj`

---

## 1. KRİTİK GÜVENLİK AÇIKLARI

### 1.1 API — Şifre Düz Metin Karşılaştırması (Fonksiyonel + Güvenlik Hatası)
**Dosya:** `OgrenciBilgiSistemi.Api/Services/LoginService.cs:31`

```sql
AND K.Sifre = @password
```

MVC uygulaması şifreleri `ASP.NET Core PasswordHasher` ile hash'leyerek saklar.
API ise gelen ham şifreyi hash ile doğrudan SQL'de karşılaştırır.
Sonuç: **API üzerinden hiçbir kullanıcı giriş yapamaz** ve bu aynı zamanda kritik bir güvenlik açığıdır.
Hash karşılaştırması için `PasswordHasher.VerifyHashedPassword()` kullanılmalıdır.

---

### 1.2 API — Kaynak Kodda Gömülü Veritabanı Kimlik Bilgileri
**Dosya:** `OgrenciBilgiSistemi.Api/appsettings.json:3`

```
Server=192.168.1.53,1433;Database=OgrenciBilgiSistemiDb;User ID=netron;Password=Netron.2016;TrustServerCertificate=True;
```

Gerçek sunucu IP'si, kullanıcı adı ve şifre kaynak koda işlenmiş durumda.
`appsettings.json` `.gitignore`'a eklenmeli; bağlantı dizesi environment variable veya secrets yöneticisi ile sağlanmalıdır.

---

### 1.3 API — Kimlik Doğrulama / Yetkilendirme Yok
**Dosya:** `OgrenciBilgiSistemi.Api/Program.cs`

- `app.UseAuthentication()` hiç eklenmemiş.
- `app.UseAuthorization()` satırı var ama `UseAuthentication()` olmadan çalışmaz.
- Tüm API endpoint'leri (öğrenci listesi, sınıf bilgileri, birimler) kimlik doğrulama olmadan herkese açık.
- JWT veya API Key tabanlı kimlik doğrulama entegre edilmelidir.

---

### 1.4 API — HTTPS Yönlendirmesi Devre Dışı
**Dosya:** `OgrenciBilgiSistemi.Api/Program.cs:37`

```csharp
// app.UseHttpsRedirection();
```

"Test aşaması" yorumuyla yorum satırı yapılmış ancak production'da da kaldırılmamış.
Tüm API trafiği şifresiz gidebilir.

---

### 1.5 API — CORS Her Kaynağa Açık
**Dosya:** `OgrenciBilgiSistemi.Api/Program.cs:8-11`

```csharp
builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
```

Kimlik doğrulama ve HTTPS olmadan bu yapılandırma; herhangi bir web sitesinin API'ye istek atmasına izin verir.
CORS politikası yalnızca bilinen origin'lerle kısıtlanmalıdır.

---

### 1.6 MVC — Açık Yönlendirme (Open Redirect)
**Dosya:** `OgrenciBilgiSistemi/Controllers/OgrenciAidatController.cs:339-340`

```csharp
if (!string.IsNullOrWhiteSpace(returnUrl))
    return Redirect(returnUrl);   // ← HATALI: harici URL'ye yönlendirme mümkün
```

`OdemeSil` action'ı `returnUrl` parametresini `Url.IsLocalUrl()` kontrolü yapmadan kullanıyor.
Aynı dosyada `MuafiyetAyarla` action'ı doğru şekilde kontrol ediyor; `OdemeSil`'de de aynı kontrol yapılmalı.

---

### 1.7 MVC — GET İsteğiyle Durum Değiştirme (CSRF Riski)
**Dosya:** `OgrenciBilgiSistemi/Controllers/OgrenciAidatController.cs:397-413`

```csharp
[HttpGet("MuafiyetAyarla")]
public async Task<IActionResult> MuafiyetAyarlaGet(...)
{
    var ok = await _aidatService.SetYillikMuafiyetAsync(...); // Durum değiştirir!
```

GET isteği veritabanında değişiklik yapıyor. GET istekleri idempotent olmalıdır.
`[ValidateAntiForgeryToken]` GET isteklerinde çalışmaz, bu nedenle CSRF saldırısına açıktır.

---

### 1.8 MVC — Çıkış (Logout) GET ile Yapılıyor
**Dosya:** `OgrenciBilgiSistemi/Controllers/HesaplarController.cs:86-91`

```csharp
[Authorize]
public async Task<IActionResult> Cikis()
{
    await HttpContext.SignOutAsync(...);
```

`Cikis` action'ı `[HttpGet]` (default). Logout işlemleri POST + CSRF token ile korunmalıdır.
Aksi hâlde saldırgan bir link ile kullanıcının oturumu zorla kapatılabilir.

---

## 2. ORTA ÖNEMLİ HATALAR

### 2.1 Eksik Benzersiz İndeksler (Veritabanı Bütünlüğü)
**Dosya:** `OgrenciBilgiSistemi/Data/AppDbContext.cs`

Aşağıdaki alanlarda veritabanı düzeyinde benzersizlik kısıtı yok:

| Tablo | Alan | Risk |
|-------|------|------|
| `Ogrenciler` | `OgrenciNo` | Aynı numara iki öğrencide olabilir |
| `Ogrenciler` | `OgrenciKartNo` | Aynı kart iki öğrenciye atanabilir |
| `Kullanicilar` | `KullaniciAdi` | Uygulama kodu kontrol eder ama DB garantisi yok (race condition) |

Tüm benzersizlik kontrolleri yalnızca uygulama katmanında; eşzamanlı isteklerde çakışma oluşabilir.

---

### 2.2 Kullanılmayan Değişken — `girisMi`
**Dosya:** `OgrenciBilgiSistemi/Services/BackgroundServices/CardReadEventHandlerService.cs:83`

```csharp
var girisMi = !acikVar; // açık yoksa GİRİŞ, varsa ÇIKIŞ
// ↑ Bu değişken hiç kullanılmıyor! Yön kararı GecisService'e bırakılmış.
```

76-83. satırlar arasındaki `acikVar` sorgusu ve `girisMi` hesabı tamamen ölü koddur.
Aynı yön mantığı `GecisService.KaydetAsync` içinde de yapılmaktadır (gereksiz çift sorgu).

---

### 2.3 Namespace Tutarsızlığı — API Projesi
**Dosyalar:** `OgrenciBilgiSistemi.Api/` altındaki tüm dosyalar

Proje adı `OgrenciBilgiSistemi.Api` iken kod tamamı `StudentTrackingSystem.Api` namespace'i kullanıyor:

```csharp
// Program.cs:1
using StudentTrackingSystem.Api.Services;

// Controllers/AuthController.cs:2-3
using StudentTrackingSystem.Api.Models;
namespace StudentTrackingSystem.Api.Controllers
```

Bu, projenin kopyalanıp yeniden adlandırıldığına işaret eder. Namespace'ler proje adıyla tutarlı olmalıdır.

---

### 2.4 Namespace Eksikliği — Bazı Sınıflar
**Dosyalar:**
- `OgrenciBilgiSistemi/Services/BackgroundServices/CardReadEventHandlerService.cs` — namespace yok
- `OgrenciBilgiSistemi/Infrastructure/FileStorage/LocalFileStorage.cs` — namespace yok
- `OgrenciBilgiSistemi/Controllers/HomeController.cs` — namespace yok

Diğer tüm sınıflar namespace tanımlarken bu üç dosya tanımlamamış. Tutarsız ve olası isim çakışması riski taşır.

---

### 2.5 `OgrenciNo` Sıfır Değer Kabul Ediyor
**Dosya:** `OgrenciBilgiSistemi/Models/OgrenciModel.cs:22`

```csharp
[Required]
public int OgrenciNo { get; set; }
```

`int` tipi için `[Required]` değer 0 olsa bile geçer (0 varsayılan değerdir).
`[Range(1, int.MaxValue, ErrorMessage = "Geçerli bir öğrenci numarası giriniz.")]` eklenmelidir.

---

### 2.6 Şifre Minimum Uzunluk Kısıtı Yok
**Dosya:** `OgrenciBilgiSistemi/Models/KullaniciModel.cs:17-19`

```csharp
[Required(ErrorMessage = "Şifre gereklidir.")]
[DataType(DataType.Password)]
public string Sifre { get; set; } = string.Empty;
```

`[MinLength]` veya `[StringLength(minLength: 6)]` gibi bir kısıt yok. 1 karakterli şifre kabul edilir.

---

### 2.7 Dosya Yükleme — Magic Byte Doğrulaması Yapılmıyor
**Dosya:** `OgrenciBilgiSistemi/Infrastructure/FileStorage/LocalFileStorage.cs:12`

```csharp
var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
if (!_allowed.Contains(ext)) throw ...
```

Yalnızca dosya uzantısı kontrol ediliyor; dosyanın gerçek içeriği (magic bytes) doğrulanmıyor.
`.aspx` uzantısı `.jpg` olarak yeniden adlandırılıp upload edilebilir.

---

### 2.8 Login için Entity Modeli Kullanılıyor
**Dosya:** `OgrenciBilgiSistemi/Controllers/HesaplarController.cs:29`

```csharp
public async Task<IActionResult> Giris(KullaniciModel model)
```

`KullaniciModel` hem DB entity'si hem de login formu modeli olarak kullanılıyor.
Ayrı bir `LoginDto` (`KullaniciAdi`, `Sifre`, `BeniHatirla`) oluşturulmalı.
Mevcut yapıda model binding tüm `KullaniciModel` alanlarını deserialize eder (güvenlik riski).

---

### 2.9 Login Brute-Force Koruması Yok
**Dosya:** `OgrenciBilgiSistemi/Controllers/HesaplarController.cs`

Başarısız giriş sayısı takip edilmiyor; rate limiting yok.
Projede `Polly` kütüphanesi mevcut ancak login koruması için kullanılmamış.
ASP.NET Core'un `IDistributedCache` veya `IMemoryCache` ile basit bir deneme sayacı eklenebilir.

---

### 2.10 API — `AuthenticateAsync` Dönüş Tipi Nullable Değil
**Dosya:** `OgrenciBilgiSistemi.Api/Services/LoginService.cs:16-63`

```csharp
public async Task<User> AuthenticateAsync(string username, string password)
{
    ...
    return null; // ← NullReferenceException riski
}
```

Metot imzası `Task<User>` ancak `null` dönebiliyor. `Task<User?>` olmalı.

---

## 3. EKSİK ÖZELLİKLER

### 3.1 `OgretmenModel` için `DbSet` Tanımlanmamış
**Dosya:** `OgrenciBilgiSistemi/Data/AppDbContext.cs`

`OgretmenModel.cs` entity sınıfı var, Views/Ogretmenler klasörü var (Ekle, Guncelle, Index, Detay),
ancak `AppDbContext`'te `DbSet<OgretmenModel> Ogretmenler` tanımlanmamış.
Öğretmenler muhtemelen `PersonelModel` üzerinden yönetiliyor ama bu ilişki net değil.

---

### 3.2 API — Token Tabanlı Kimlik Doğrulama Yok
**Dosya:** `OgrenciBilgiSistemi.Api/Controllers/AuthController.cs`

Başarılı login'de kullanıcı nesnesi döndürülüyor ama JWT veya benzeri bir token üretilmiyor.
Sonraki API çağrılarında kimlik kanıtlanamamaktadır.

---

### 3.3 Dashboard — Aktif Öğrenci Sayısı Yanlış
**Dosya:** `OgrenciBilgiSistemi/Controllers/HomeController.cs:42`

```csharp
var toplamOgrenci = await _db.Ogrenciler.AsNoTracking().CountAsync();
```

Global Query Filter (`OgrenciDurum == true`) bu sorguyu filtrelemez çünkü
`IncludePasifOgrenciler` false olduğunda pasif öğrenciler zaten filtrelenecek.
Ancak `HomeController`'da `AppDbContext.IncludePasifOgrenciler` ayarlanmadığından
global filtre devreye girer ve doğru çalışır. Fakat `DashboardStatsDto.ToplamOgrenci`'nin
"aktif öğrenci sayısı" mı yoksa "toplam" mı olduğu belgelenmemiş.

---

### 3.4 Eksik View — `YetkisizGiris`
**Dosya:** `OgrenciBilgiSistemi/Controllers/HesaplarController.cs:93-94`

```csharp
public IActionResult YetkisizGiris() => View();
```

Cookie auth `AccessDeniedPath = "/Hesaplar/YetkisizGiris"` olarak ayarlı.
Ancak bu view (Views/Hesaplar/YetkisizGiris.cshtml) dosyası mevcut değil;
404 hatası yerine 500 hatası döner.

---

### 3.5 OgrenciAidat — `Index` ve `AidatRapor` Kod Tekrarı
**Dosya:** `OgrenciBilgiSistemi/Controllers/OgrenciAidatController.cs:29-125` ve `161-261`

`Index` ve `AidatRapor` action'ları neredeyse aynı kodu tekrar ediyor (dropdown oluşturma,
rapor çekme, ViewModel doldurma). Bu iki action ortak bir private metoda refactor edilmelidir.

---

### 3.6 AppDbContext — `IncludePasifOgrenciler` ile Controller Arasında Kopukluk
**Dosya:** `OgrenciBilgiSistemi/Controllers/OgrencilerController.cs:23` ve `OgrenciBilgiSistemi/Data/AppDbContext.cs:10`

`OgrencilerController.IncludePasifOgrenciler` property'si her zaman `false` (değiştirilmiyor).
`AppDbContext.IncludePasifOgrenciler` de hiçbir yerde `true` yapılmıyor.
Pasif öğrencileri görmek için tasarlanan mekanizma kullanılmıyor/eksik.

---

## 4. KOD KALİTESİ

### 4.1 Eksik `CancellationToken` Kullanımı
`KullanicilarController.Detay(int? id)` — `CancellationToken` almıyor.
`KullanicilarController.Ekle()`, `KullanicilarController.Guncelle()` — async ama `CancellationToken` yok.

### 4.2 `using var` Yerine `using (var ...)` Kullanımı
`OgrenciBilgiSistemi.Api/Services/LoginService.cs` — `using (SqlConnection ...)` eski stil.
Modern C# `using var` sözdizimi kullanılmalı.

### 4.3 Genel `Exception` Yakalanıyor
`LoginService.cs:57-60`:
```csharp
catch (Exception ex)
{
    throw new Exception("Veritabanı bağlantı hatası: " + ex.Message);
```
`Exception` mesajı ile wrap etmek stack trace'i kaybettirir. `throw;` veya özel exception tipi kullanılmalı.

### 4.4 `HomeController` `_logger` Bağımlılığı Yok
`HomeController` logger kullanmıyor ve hata durumları yönetilmiyor.
Dashboard endpoint'lerinde DB hatası oluşursa 500 döner.

---

## 5. ÖZET TABLO

| # | Kategori | Sorun | Öncelik |
|---|----------|-------|---------|
| 1.1 | Güvenlik | API şifre düz metin karşılaştırma | **KRİTİK** |
| 1.2 | Güvenlik | API'de gömülü DB kimlik bilgileri | **KRİTİK** |
| 1.3 | Güvenlik | API kimlik doğrulama yok | **KRİTİK** |
| 1.4 | Güvenlik | API HTTPS devre dışı | **KRİTİK** |
| 1.5 | Güvenlik | CORS her kaynağa açık | **KRİTİK** |
| 1.6 | Güvenlik | Open Redirect — OdemeSil | **Yüksek** |
| 1.7 | Güvenlik | GET ile durum değişimi (CSRF) | **Yüksek** |
| 1.8 | Güvenlik | Logout GET isteğiyle yapılıyor | **Yüksek** |
| 2.1 | Hata | Eksik DB benzersiz indeksleri | **Yüksek** |
| 2.2 | Hata | Kullanılmayan `girisMi` değişkeni | **Düşük** |
| 2.3 | Hata | API namespace tutarsızlığı | **Orta** |
| 2.4 | Hata | Namespace eksikliği (3 dosya) | **Düşük** |
| 2.5 | Hata | OgrenciNo 0 değer kabul ediyor | **Orta** |
| 2.6 | Hata | Şifre minimum uzunluk yok | **Orta** |
| 2.7 | Güvenlik | Dosya yükleme magic byte kontrolü yok | **Orta** |
| 2.8 | Tasarım | Login için entity modeli kullanılıyor | **Orta** |
| 2.9 | Güvenlik | Login brute-force koruması yok | **Orta** |
| 2.10 | Hata | `AuthenticateAsync` nullable dönüş tipi | **Düşük** |
| 3.1 | Eksik | `OgretmenModel` DbSet tanımlanmamış | **Yüksek** |
| 3.2 | Eksik | API token kimlik doğrulama yok | **Yüksek** |
| 3.3 | Eksik | Dashboard aktif öğrenci sayısı belirsiz | **Düşük** |
| 3.4 | Eksik | `YetkisizGiris.cshtml` view yok | **Yüksek** |
| 3.5 | Kod Kalitesi | Index ve AidatRapor kod tekrarı | **Düşük** |
| 3.6 | Eksik | Pasif öğrenci filtresi çalışmıyor | **Orta** |

---

## 6. ÖNERİLEN DÜZELTME SIRASI

1. **KRİTİK (hemen):** API kimlik bilgilerini `.gitignore` + environment variable'a taşı
2. **KRİTİK (hemen):** API'ye JWT authentication ekle
3. **KRİTİK (hemen):** API `LoginService` şifre doğrulamasını `PasswordHasher` ile düzelt
4. **KRİTİK (hemen):** API HTTPS yönlendirmesini aç
5. **Yüksek:** `OdemeSil`'de `Url.IsLocalUrl()` kontrolü ekle
6. **Yüksek:** `Cikis` action'ını POST + CSRF token ile koru
7. **Yüksek:** `MuafiyetAyarlaGet` — GET'ten POST'a taşı
8. **Yüksek:** `YetkisizGiris.cshtml` view dosyasını oluştur
9. **Yüksek:** `OgrenciNo` ve `OgrenciKartNo` için unique index ekle
10. **Orta:** Şifre minimum uzunluk validasyonu ekle
11. **Orta:** Dosya yükleme magic byte doğrulaması ekle
12. **Orta:** Login için ayrı `LoginDto` oluştur
13. **Orta:** Pasif öğrenci filtresi mekanizmasını düzelt
14. **Orta:** API namespace'lerini proje adıyla tutarlı hâle getir
15. **Düşük:** Kullanılmayan `girisMi` değişkenini ve ölü kodu temizle
