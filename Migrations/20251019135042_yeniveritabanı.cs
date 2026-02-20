using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class yeniveritabanı : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Birimler",
                columns: table => new
                {
                    BirimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BirimAd = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirimDurum = table.Column<bool>(type: "bit", nullable: false),
                    BirimSinifMi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Birimler", x => x.BirimId);
                });

            migrationBuilder.CreateTable(
                name: "Cihazlar",
                columns: table => new
                {
                    CihazId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CihazAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CihazKodu = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonanimTipi = table.Column<byte>(type: "tinyint", nullable: false),
                    IstasyonTipi = table.Column<short>(type: "smallint", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    IpAdresi = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    PortNo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cihazlar", x => x.CihazId);
                });

            migrationBuilder.CreateTable(
                name: "Kitaplar",
                columns: table => new
                {
                    KitapId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KitapAd = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KitapGorsel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KitapTurAd = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    KitapGun = table.Column<int>(type: "int", nullable: false),
                    KitapDurum = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kitaplar", x => x.KitapId);
                });

            migrationBuilder.CreateTable(
                name: "MenuOgeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Baslik = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Controller = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GerekliRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sirala = table.Column<int>(type: "int", nullable: false),
                    AnaMenuId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuOgeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuOgeler_MenuOgeler_AnaMenuId",
                        column: x => x.AnaMenuId,
                        principalTable: "MenuOgeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciAidatTarifeler",
                columns: table => new
                {
                    OgrenciAidatTarifeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaslangicYil = table.Column<int>(type: "int", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciAidatTarifeler", x => x.OgrenciAidatTarifeId);
                    table.CheckConstraint("CK_Tarife_BaslangicYil", "[BaslangicYil] BETWEEN 2000 AND 2100");
                    table.CheckConstraint("CK_Tarife_Tutar", "[Tutar] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    KullaniciId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BeniHatirla = table.Column<bool>(type: "bit", nullable: false),
                    AdminMi = table.Column<bool>(type: "bit", nullable: false),
                    KullaniciDurum = table.Column<bool>(type: "bit", nullable: false),
                    BirimId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.KullaniciId);
                    table.ForeignKey(
                        name: "FK_Kullanicilar_Birimler_BirimId",
                        column: x => x.BirimId,
                        principalTable: "Birimler",
                        principalColumn: "BirimId");
                });

            migrationBuilder.CreateTable(
                name: "Personeller",
                columns: table => new
                {
                    PersonelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonelAdSoyad = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PersonelGorselPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PersonelDurum = table.Column<bool>(type: "bit", nullable: false),
                    PersonelTipi = table.Column<byte>(type: "tinyint", nullable: false),
                    PersonelKartNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BirimId = table.Column<int>(type: "int", nullable: true),
                    PersonelEmail = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    PersonelTelefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personeller", x => x.PersonelId);
                    table.ForeignKey(
                        name: "FK_Personeller_Birimler_BirimId",
                        column: x => x.BirimId,
                        principalTable: "Birimler",
                        principalColumn: "BirimId");
                });

            migrationBuilder.CreateTable(
                name: "KullaniciMenuOgeler",
                columns: table => new
                {
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    MenuOgeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciMenuOgeler", x => new { x.KullaniciId, x.MenuOgeId });
                    table.ForeignKey(
                        name: "FK_KullaniciMenuOgeler_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "KullaniciId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KullaniciMenuOgeler_MenuOgeler_MenuOgeId",
                        column: x => x.MenuOgeId,
                        principalTable: "MenuOgeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ogrenciler",
                columns: table => new
                {
                    OgrenciId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciAdSoyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OgrenciNo = table.Column<int>(type: "int", nullable: false),
                    OgrenciGorsel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OgrenciKartNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OgrenciVeliAdSoyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OgrenciVeliTelefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    OgrenciCikisDurumu = table.Column<int>(type: "int", nullable: false),
                    OgrenciDurum = table.Column<bool>(type: "bit", nullable: false),
                    PersonelId = table.Column<int>(type: "int", nullable: true),
                    BirimId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ogrenciler", x => x.OgrenciId);
                    table.ForeignKey(
                        name: "FK_Ogrenciler_Birimler_BirimId",
                        column: x => x.BirimId,
                        principalTable: "Birimler",
                        principalColumn: "BirimId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ogrenciler_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "PersonelId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PersonelDetaylar",
                columns: table => new
                {
                    PersonelDetayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonelId = table.Column<int>(type: "int", nullable: false),
                    IstasyonTipi = table.Column<short>(type: "smallint", nullable: false),
                    PersonelGTarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PersonelCTarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PersonelGecisTipi = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    PersonelSmsGonderildi = table.Column<bool>(type: "bit", nullable: true),
                    PersonelResimYolu = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    CihazId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonelDetaylar", x => x.PersonelDetayId);
                    table.ForeignKey(
                        name: "FK_PersonelDetaylar_Cihazlar_CihazId",
                        column: x => x.CihazId,
                        principalTable: "Cihazlar",
                        principalColumn: "CihazId");
                    table.ForeignKey(
                        name: "FK_PersonelDetaylar_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "PersonelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KitapDetaylar",
                columns: table => new
                {
                    KitapDetayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KitapAlTarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KitapVerTarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    KitapDurum = table.Column<int>(type: "int", nullable: false),
                    KitapId = table.Column<int>(type: "int", nullable: false),
                    OgrenciId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitapDetaylar", x => x.KitapDetayId);
                    table.ForeignKey(
                        name: "FK_KitapDetaylar_Kitaplar_KitapId",
                        column: x => x.KitapId,
                        principalTable: "Kitaplar",
                        principalColumn: "KitapId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KitapDetaylar_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "OgrenciId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciAidatlar",
                columns: table => new
                {
                    OgrenciAidatId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<int>(type: "int", nullable: false),
                    BaslangicYil = table.Column<int>(type: "int", nullable: false),
                    Borc = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Odenen = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Muaf = table.Column<bool>(type: "bit", nullable: false),
                    SonOdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciAidatlar", x => x.OgrenciAidatId);
                    table.CheckConstraint("CK_Aidat_BaslangicYil", "[BaslangicYil] BETWEEN 2000 AND 2100");
                    table.CheckConstraint("CK_Aidat_Pozitif", "[Borc] >= 0 AND [Odenen] >= 0");
                    table.ForeignKey(
                        name: "FK_OgrenciAidatlar_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "OgrenciId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciDetaylar",
                columns: table => new
                {
                    OgrenciDetayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<int>(type: "int", nullable: false),
                    IstasyonTipi = table.Column<short>(type: "smallint", nullable: false),
                    OgrenciGTarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OgrenciCTarih = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OgrenciGecisTipi = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    OgrenciSmsGonderildi = table.Column<bool>(type: "bit", nullable: true),
                    OgrenciResimYolu = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    CihazId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciDetaylar", x => x.OgrenciDetayId);
                    table.ForeignKey(
                        name: "FK_OgrenciDetaylar_Cihazlar_CihazId",
                        column: x => x.CihazId,
                        principalTable: "Cihazlar",
                        principalColumn: "CihazId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OgrenciDetaylar_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "OgrenciId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciYemekler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<int>(type: "int", nullable: false),
                    Yil = table.Column<int>(type: "int", nullable: false),
                    Ay = table.Column<int>(type: "int", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Not = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciYemekler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciYemekler_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "OgrenciId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciYemekOdemeler",
                columns: table => new
                {
                    OgrenciYemekOdemeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<int>(type: "int", nullable: false),
                    Yil = table.Column<int>(type: "int", nullable: false),
                    Ay = table.Column<int>(type: "int", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciYemekOdemeler", x => x.OgrenciYemekOdemeId);
                    table.ForeignKey(
                        name: "FK_OgrenciYemekOdemeler_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "OgrenciId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciYemekTarifeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciId = table.Column<int>(type: "int", nullable: false),
                    Yil = table.Column<int>(type: "int", nullable: false),
                    AylikTutar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciYemekTarifeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OgrenciYemekTarifeler_Ogrenciler_OgrenciId",
                        column: x => x.OgrenciId,
                        principalTable: "Ogrenciler",
                        principalColumn: "OgrenciId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OgrenciAidatOdemeler",
                columns: table => new
                {
                    OgrenciAidatOdemeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OgrenciAidatId = table.Column<int>(type: "int", nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OdemeTipi = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciAidatOdemeler", x => x.OgrenciAidatOdemeId);
                    table.CheckConstraint("CK_AidatOdeme_Tutar_NonNegative", "[Tutar] >= 0");
                    table.ForeignKey(
                        name: "FK_OgrenciAidatOdemeler_OgrenciAidatlar_OgrenciAidatId",
                        column: x => x.OgrenciAidatId,
                        principalTable: "OgrenciAidatlar",
                        principalColumn: "OgrenciAidatId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MenuOgeler",
                columns: new[] { "Id", "Action", "AnaMenuId", "Baslik", "Controller", "GerekliRole", "Sirala" },
                values: new object[,]
                {
                    { 1, "Index", null, "Ana Sayfa", "Home", null, 1 },
                    { 2, null, null, "Personeller", null, null, 2 },
                    { 5, null, null, "Öğrenciler", null, null, 3 },
                    { 10, null, null, "Kullanıcılar", null, null, 4 },
                    { 13, null, null, "Kitaplar", null, null, 5 },
                    { 16, null, null, "Cihazlar", null, null, 6 },
                    { 18, null, null, "Raporlar", null, null, 7 },
                    { 22, null, null, "KartOku", null, null, 8 },
                    { 3, "Index", 2, "Birim Listesi", "Birimler", null, 1 },
                    { 4, "Index", 2, "Personel Listesi", "Personeller", null, 2 },
                    { 7, "Index", 5, "Öğrenci İşlemleri", "Ogrenciler", null, 1 },
                    { 8, "Index", 5, "Aidat İşlemleri", "OgrenciAidat", null, 2 },
                    { 9, "Index", 5, "Yemekhane İşlemleri", "OgrenciYemekhane", null, 3 },
                    { 12, "Index", 10, "Kullanıcı Listesi", "Kullanicilar", null, 1 },
                    { 14, "Index", 13, "Kitap Listesi", "Kitaplar", null, 1 },
                    { 15, "Index", 13, "Kitap Hareketleri", "KitapDetaylar", null, 2 },
                    { 17, "Index", 16, "Cihaz Listesi", "Cihazlar", null, 1 },
                    { 19, "Detay", 18, "Öğrenci Giriş Çıkış Raporları", "OgrenciGirisCikis", null, 1 },
                    { 20, "AidatRapor", 18, "Öğrenci Aidat Raporu", "OgrenciAidat", null, 2 },
                    { 21, "YemekRapor", 18, "Öğrenci Yemek Raporu", "OgrenciYemekhane", null, 3 },
                    { 23, "Index", 22, "Kart Okuma Ekranı", "KartOku", null, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cihazlar_CihazAdi",
                table: "Cihazlar",
                column: "CihazAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cihazlar_CihazKodu",
                table: "Cihazlar",
                column: "CihazKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cihazlar_IstasyonTipi",
                table: "Cihazlar",
                column: "IstasyonTipi");

            migrationBuilder.CreateIndex(
                name: "IX_KitapDetaylar_KitapId",
                table: "KitapDetaylar",
                column: "KitapId");

            migrationBuilder.CreateIndex(
                name: "IX_KitapDetaylar_OgrenciId",
                table: "KitapDetaylar",
                column: "OgrenciId");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_BirimId",
                table: "Kullanicilar",
                column: "BirimId");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciMenuOgeler_MenuOgeId",
                table: "KullaniciMenuOgeler",
                column: "MenuOgeId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuOgeler_AnaMenuId_Sirala",
                table: "MenuOgeler",
                columns: new[] { "AnaMenuId", "Sirala" });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciAidatlar_OgrenciId_BaslangicYil",
                table: "OgrenciAidatlar",
                columns: new[] { "OgrenciId", "BaslangicYil" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciAidatOdemeler_OgrenciAidatId_OdemeTarihi",
                table: "OgrenciAidatOdemeler",
                columns: new[] { "OgrenciAidatId", "OdemeTarihi" });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciAidatTarifeler_BaslangicYil",
                table: "OgrenciAidatTarifeler",
                column: "BaslangicYil",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciDetaylar_CihazId",
                table: "OgrenciDetaylar",
                column: "CihazId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciDetaylar_OgrenciCTarih",
                table: "OgrenciDetaylar",
                column: "OgrenciCTarih");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciDetaylar_OgrenciGTarih",
                table: "OgrenciDetaylar",
                column: "OgrenciGTarih");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciDetaylar_OgrenciId_IstasyonTipi",
                table: "OgrenciDetaylar",
                columns: new[] { "OgrenciId", "IstasyonTipi" });

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_BirimId",
                table: "Ogrenciler",
                column: "BirimId");

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_PersonelId",
                table: "Ogrenciler",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciYemekler_OgrenciId_Yil_Ay",
                table: "OgrenciYemekler",
                columns: new[] { "OgrenciId", "Yil", "Ay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciYemekOdemeler_OgrenciId_Yil_Ay",
                table: "OgrenciYemekOdemeler",
                columns: new[] { "OgrenciId", "Yil", "Ay" });

            migrationBuilder.CreateIndex(
                name: "IX_OgrenciYemekTarifeler_OgrenciId_Yil",
                table: "OgrenciYemekTarifeler",
                columns: new[] { "OgrenciId", "Yil" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonelDetaylar_CihazId",
                table: "PersonelDetaylar",
                column: "CihazId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonelDetaylar_PersonelId",
                table: "PersonelDetaylar",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_BirimId",
                table: "Personeller",
                column: "BirimId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KitapDetaylar");

            migrationBuilder.DropTable(
                name: "KullaniciMenuOgeler");

            migrationBuilder.DropTable(
                name: "OgrenciAidatOdemeler");

            migrationBuilder.DropTable(
                name: "OgrenciAidatTarifeler");

            migrationBuilder.DropTable(
                name: "OgrenciDetaylar");

            migrationBuilder.DropTable(
                name: "OgrenciYemekler");

            migrationBuilder.DropTable(
                name: "OgrenciYemekOdemeler");

            migrationBuilder.DropTable(
                name: "OgrenciYemekTarifeler");

            migrationBuilder.DropTable(
                name: "PersonelDetaylar");

            migrationBuilder.DropTable(
                name: "Kitaplar");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "MenuOgeler");

            migrationBuilder.DropTable(
                name: "OgrenciAidatlar");

            migrationBuilder.DropTable(
                name: "Cihazlar");

            migrationBuilder.DropTable(
                name: "Ogrenciler");

            migrationBuilder.DropTable(
                name: "Personeller");

            migrationBuilder.DropTable(
                name: "Birimler");
        }
    }
}
