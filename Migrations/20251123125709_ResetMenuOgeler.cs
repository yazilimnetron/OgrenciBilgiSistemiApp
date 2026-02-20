using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class ResetMenuOgeler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Eski menüleri temizle
            migrationBuilder.Sql("DELETE FROM [MenuOgeler];");

            // 2) IDENTITY_INSERT aç
            migrationBuilder.Sql("SET IDENTITY_INSERT [MenuOgeler] ON;");

            // 3) Tüm menü kayıtlarını tek seferde ekle
            migrationBuilder.Sql(@"
INSERT INTO [MenuOgeler] ([Id], [Action], [AnaMenuId], [Baslik], [Controller], [GerekliRole], [Sirala])
VALUES 
(1,  'Index',  NULL, N'Ana Sayfa',               'Home',          NULL, 1),

(2,  NULL,     NULL, N'Personeller',             NULL,            NULL, 2),
(3,  'Index',  2,    N'Birim Listesi',           'Birimler',      NULL, 1),
(4,  'Index',  2,    N'Personel Listesi',        'Personeller',   NULL, 2),

(5,  NULL,     NULL, N'Öğrenciler',              NULL,            NULL, 3),
(6,  'Index',  5,    N'Öğrenci İşlemleri',       'Ogrenciler',    NULL, 1),
(7,  'Index',  5,    N'Aidat İşlemleri',         'OgrenciAidat',  NULL, 2),
(8,  'Index',  5,    N'Yemekhane İşlemleri',     'OgrenciYemekhane', NULL, 3),

(9,  NULL,     NULL, N'Ziyaretçiler',            NULL,            NULL, 8),
(10, 'Index',  9,    N'Ziyaretçi İşlemleri',     'Ziyaretci',     NULL, 1),

(11, NULL,     NULL, N'Kullanıcılar',            NULL,            NULL, 4),
(12, 'Index',  11,   N'Kullanıcı Listesi',       'Kullanicilar',  NULL, 1),

(13, NULL,     NULL, N'Kitaplar',                NULL,            NULL, 5),
(14, 'Index',  13,   N'Kitap Listesi',           'Kitaplar',      NULL, 1),
(15, 'Index',  13,   N'Kitap Hareketleri',       'KitapDetaylar', NULL, 2),

(16, NULL,     NULL, N'Cihazlar',                NULL,            NULL, 6),
(17, 'Index',  16,   N'Cihaz Listesi',           'Cihazlar',      NULL, 1),

(18, NULL,     NULL, N'Raporlar',                NULL,            NULL, 7),
(19, 'Detay',  18,   N'Öğrenci Giriş Çıkış Raporları', 'OgrenciGirisCikis', NULL, 1),
(20, 'OgrenciVeliRapor', 18, N'Öğrenci Veli Raporu',    'Ogrenciler',     NULL, 2),
(21, 'AidatRapor', 18,      N'Öğrenci Aidat Raporu',    'OgrenciAidat',   NULL, 3),
(22, 'YemekRapor', 18,      N'Öğrenci Yemek Raporu',    'OgrenciYemekhane', NULL, 4),

(23, NULL,     NULL, N'KartOku',                 NULL,            NULL, 8),
(24, 'Index',  23,   N'Kart Okuma Ekranı',       'KartOku',       NULL, 1);
");

            // 4) IDENTITY_INSERT kapat
            migrationBuilder.Sql("SET IDENTITY_INSERT [MenuOgeler] OFF;");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
