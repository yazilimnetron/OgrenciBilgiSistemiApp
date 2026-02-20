# OgrenciBilgiSistemiApp — Derinlemesine Teknik Analiz

## 1) Uygulama Kimliği ve Kapsam
- Proje, **ASP.NET Core MVC (`net9.0`)** üzerinde çalışan, okul/kurum operasyonlarını tek bir monolitte toplayan bir bilgi sistemi.
- Fonksiyonel kapsam: öğrenci, veli, aidat, yemekhane, ziyaretçi, personel, kitap, cihaz, kullanıcı/yetki ve dashboard/raporlama.
- Donanım entegrasyonu: ZKTeco biyometrik cihaz okuma/yazma senaryoları.

## 2) Mimari Fotoğraf (Mevcut Durum)

### 2.1 Katmanlar
- **Controller katmanı**: HTTP isteklerini alıyor, ViewModel kuruyor, servisleri orkestre ediyor.
- **Service katmanı**: İş kuralları, raporlama sorguları, dış sistem (cihaz) entegrasyonu.
- **Data katmanı**: `AppDbContext` + EF Core fluent config + migration tabanlı şema evrimi.
- **UI katmanı**: Razor Views + ViewComponent tabanlı dinamik menü.

### 2.2 Bileşen sayısı (hızlı ölçek göstergesi)
- Controller: **15**
- Service implementasyonu: **15**
- Model: **29**
- View: **65**
- Migration: **19**

> Bu ölçek, sistemin “küçük CRUD” seviyesini geçtiğini; bakım disiplini, standartlaştırma ve test otomasyonuna ihtiyaç duyduğunu gösteriyor.

## 3) Runtime ve Altyapı Tasarımı

### 3.1 Program başlangıcı ve middleware
- Uygulama startup’ında:
  - `AddDbContextPool<AppDbContext>` ile SQL Server bağlantısı.
  - Global `AuthorizeFilter` ile tüm MVC endpointlerinin varsayılan olarak kimlik doğrulamaya alınması.
  - Cookie auth (`ExpireTimeSpan=8 saat`, sliding enabled).
  - SignalR hub mapping (`/kartOkuHub`).
  - `BootstrapAdmin` yapılandırması üzerinden admin seed akışı (config boşsa kullanıcı üretmiyor, sadece uyarı logluyor).

### 3.2 AuthN / AuthZ yaklaşımı
- Güçlü taraf:
  - Global authorize varsayılanı, “unutulan endpoint” riskini düşürüyor.
  - Login/cikis akışı net.
- Dikkat edilmesi gereken:
  - Controller seviyesinde `[Authorize]` / policy standardı dağınık; role bazlı enforcement çoğunlukla UI/akış düzeyinde bırakılmış.
  - Sadece `HesaplarController` üzerinde az sayıda `AllowAnonymous`/`Authorize` attribute kullanımı var; sistem genelinde policy-driven matris net değil.

## 4) Veri Modeli ve EF Core Uygulaması

### 4.1 Güçlü modelleme pratikleri
- `AppDbContext` içinde ilişkiler ve `DeleteBehavior` kararları bilinçli.
- Birçok domain için check constraint ve unique index tanımları var (aidat/yemek/ödeme/tarife).
- Menü hiyerarşisi seed edilmiş, ilk kurulum kullanılabilirliği yüksek.

### 4.2 Öne çıkan veri davranışları
- `OgrenciModel` için global query filter: pasif öğrenci gizleme davranışı desteklenmiş (`IncludePasifOgrenciler` toggling).
- Aidat ve yemekhane raporları, SQL’e projekte edilen agregasyonlar ile performans odaklı kurgulanmış.

### 4.3 Veri modelinde risk / teknik borç
- Hem `DbContext` seviyesinde global filter hem servis içinde tekrar filtreleme var; uzun vadede “aynı kuralın iki farklı yerde drift etmesi” riski oluşur.
- Menü seed’i migration’a bağlı büyüdükçe, ortamlarda veri drift’i ve id çakışması riski artar (özellikle manuel DB müdahalelerinde).

## 5) Domain Modülleri Bazında İnceleme

### 5.1 Öğrenci & Veli
- `OgrencilerController` kapsamı geniş: listeleme, ekle-güncelle, toplu cihaz gönderimi, Excel export, raporlar.
- `OgrenciService` içinde transaction kullanımı ve görsel yükleme + yemekhane güncellemesi bir arada yönetiliyor.
- Risk:
  - Controller karmaşıklığı yüksek (orchestration + hata yönetimi + farklı use-case’ler tek sınıfta).
  - Action bazlı boyut arttıkça regression riski artar.

### 5.2 Aidat
- `AidatService`, rapor sorgularını tek yerde toplayıp hem paged UI hem Excel export için tekrar kullanıyor; bu iyi bir tasarım.
- Durum filtreleme (`Borçlu/Borçsuz/Muaf`) ve toplamların sayfalama öncesi hesaplanması doğru.

### 5.3 Menü / Yetki
- `MenuService`, rol + kullanıcı ataması + hiyerarşik görünürlük + cycle guard kombinasyonunu yönetiyor.
- Dikkat:
  - Dosyada eski implementasyonun büyük ölçüde yorum satırı olarak kalması okunabilirlik ve bakım maliyetini yükseltiyor.

### 5.4 Cihaz / Kart okuma
- `ZKTecoService`:
  - `SemaphoreSlim` ile bağlantı yarışlarını sınırlıyor.
  - Event double-subscription riskini ele alıyor.
  - COM release (`Marshal.FinalReleaseComObject`) ile kaynak sızıntısını azaltıyor.
- `CardReadEventHandlerService`:
  - Arka planda kart event’ini domain akışına çeviriyor.
  - Açık kayıt var/yok kararından giriş-çıkış yönü türetiyor.
- Kritik platform notu:
  - Proje `PlatformTarget=x86` ve COM reference (`zkemkeeper`) ile **Windows bağımlı**; Linux container/cloud portability kısıtlı.

## 6) UI/UX ve Sunum Katmanı
- Dinamik menü, `ViewComponent` ile claim tabanlı kullanıcıya göre çiziliyor.
- Dashboard endpoint’leri var (KPI + seri veri), frontend’de Chart.js entegrasyonu mevcut.
- Dikkat:
  - Bazı dosyalarda karakter kodlaması bozulma izleri var (örn. `GİRİŞ/ÇIKIŞ` metinleri `G�R��` şeklinde görünüyor). Bu hem kod okunabilirliğini hem log/rapor tutarlılığını etkiler.

## 7) Güvenlik Değerlendirmesi

### 7.1 Olumlu bulgular
- Şifre hashleme (`PasswordHasher`) kullanımı yerinde.
- Login endpoint’inde `ValidateAntiForgeryToken` uygulanmış.
- Varsayılan authorize filtre ile anonim erişim daraltılmış.

### 7.2 Riskler
1. **Connection string’in appsettings içinde host bazlı sabit bulunması**
   - Ortam bazlı secret yönetimi ihtiyacı var.
2. **Yetkilendirme matrisinin explicit olmaması**
   - “Hangi role hangi action” merkezi olarak belgelenmeli/uygulanmalı.
3. **Dosya yükleme güvenliği (kısmen iyi, kısmen eksik)**
   - Uzantı ve boyut kontrolü var; fakat MIME doğrulaması + anti-malware pipeline + image re-encode gibi ek katmanlar düşünülebilir.

## 8) Performans ve Ölçeklenebilirlik

### 8.1 İyi noktalar
- Birçok yerde `AsNoTracking` kullanımı mevcut.
- Rapor sorgularında DB tarafı filtre/projeksiyon tercih edilmiş.
- DbContext pool kullanımı var.

### 8.2 İyileştirme alanları
- Çok yoğun controller’larda sorgu + UI hazırlama ayrıştırılmalı (query object/application service).
- Sık kullanılan raporlarda index doğrulama checklist’i oluşturulmalı.
- Menü ve referans veriler için cache stratejisi (invalidasyonla birlikte) düşünülebilir.

## 9) Kod Kalitesi ve Bakım Gözlemleri
- Artılar:
  - Interface tabanlı servis ayrımı mevcut.
  - Domain modülleri belirgin.
- Eksiler:
  - Bazı dosyalarda eski kod blokları yorum olarak tutuluyor (temizlik ihtiyacı).
  - Bazı sınıflar “çok sorumluluklu” hale gelmiş.
  - Tutarlı kodlama/encoding standardı (UTF-8) tüm repo için garantilenmeli.

## 10) Önceliklendirilmiş İyileştirme Yol Haritası

### P0 (hemen)
1. Ortam değişkeni/secrets standardı ile connection string ve bootstrap ayarlarını production-safe hale getir.
2. Yetkilendirme matrisini çıkar; kritik controller/action’lara açık policy ataması yap.
3. Kodlama (UTF-8) temizliği ile bozuk Türkçe karakterleri düzelt.

### P1 (kısa vade)
1. `OgrencilerController` gibi yoğun controller’ları use-case bazlı application servislerine böl.
2. Yorum satırındaki eski implementasyon bloklarını kaldır.
3. Donanım entegrasyonunu adapter sınırına alıp “cihaz yokken degrade mode” testlerini artır.

### P2 (orta vade)
1. Entegrasyon testi (özellikle rapor query’leri ve auth akışları) ekle.
2. Operasyonel gözlemlenebilirlik için structured log + correlation id yaklaşımını standardize et.
3. Migration governance (seed veri sürümleme kuralı) dokümante et.

## 11) Sonuç
Uygulama işlevsel olarak güçlü, gerçek operasyon senaryolarını karşılayan ve servis/EF temelli iyi bir omurgaya sahip. En kritik ihtiyaç, güvenlik ve bakım sürdürülebilirliğini artıracak standardizasyon adımlarıdır: policy-driven yetkilendirme, controller sadeleştirme, platform bağımlılığı yönetimi ve kod hijyeni. Bu adımlar atıldığında sistem hem daha güvenli hem daha ölçeklenebilir hale gelir.
