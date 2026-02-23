using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class GuvenlikUniqueIndexler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OgrenciNo: her numara yalnızca bir öğrenciye ait olabilir
            migrationBuilder.CreateIndex(
                name: "UX_Ogrenciler_OgrenciNo",
                table: "Ogrenciler",
                column: "OgrenciNo",
                unique: true);

            // OgrenciKartNo: bir kart birden fazla öğrenciye atanamaz
            // Boş / null kartlar benzersizlik kapsamı dışında tutulur
            migrationBuilder.CreateIndex(
                name: "UX_Ogrenciler_OgrenciKartNo",
                table: "Ogrenciler",
                column: "OgrenciKartNo",
                unique: true,
                filter: "[OgrenciKartNo] IS NOT NULL AND [OgrenciKartNo] != ''");

            // KullaniciAdi: tüm kayıtlar arasında benzersiz olmalı
            migrationBuilder.CreateIndex(
                name: "UX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Ogrenciler_OgrenciNo",
                table: "Ogrenciler");

            migrationBuilder.DropIndex(
                name: "UX_Ogrenciler_OgrenciKartNo",
                table: "Ogrenciler");

            migrationBuilder.DropIndex(
                name: "UX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar");
        }
    }
}
