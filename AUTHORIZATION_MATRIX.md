# Yetkilendirme Matrisi

## Varsayılan Kurallar
- Tüm MVC endpoint'leri `Program.cs` içinde tanımlanan global `AuthorizeFilter` ile kimlik doğrulama gerektirir.
- Anonim erişim yalnızca explicit `[AllowAnonymous]` ile açılır.

## Controller Bazlı Kurallar
| Controller | Varsayılan Erişim | Ek Policy | Not |
|---|---|---|---|
| `HesaplarController` | Anonim + Auth karışık | Yok | `Giris` ve `YetkisizGiris` anonimdir, `Cikis` yetki gerektirir. |
| `KullanicilarController` | Auth | `AdminOnly` | Kullanıcı ve menü yetkisi yönetimi yalnızca admin. |
| Diğer tüm controller'lar | Auth | Yok | Giriş yapmış kullanıcı erişimi. İhtiyaca göre role bazlı policy genişletilecek. |

## Yol Haritası
1. Öğrenci/aidat/yemekhane/rapor modülleri için rol setleri (`Operator`, `Muhasebe`, `Rapor`) tanımlanacak.
2. Kritik action seviyelerinde policy attribute'ları netleştirilecek.
3. Policy kuralları için integration test eklenecek.
