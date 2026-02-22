using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class ogrenciveli1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Action", "Baslik", "Controller" },
                values: new object[] { "OgrenciVeliRapor", "Öğrenci Veli Raporu", "Ogrenci" });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Action", "Baslik", "Controller" },
                values: new object[] { "AidatRapor", "Öğrenci Aidat Raporu", "OgrenciAidat" });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { "YemekRapor", 18, "Öğrenci Yemek Raporu", "OgrenciYemekhane", 4 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { null, null, "KartOku", null, 8 });

            migrationBuilder.InsertData(
                table: "MenuOgeler",
                columns: new[] { "Id", "Action", "AnaMenuId", "Baslik", "Controller", "GerekliRole", "Sirala" },
                values: new object[] { 24, "Index", 23, "Kart Okuma Ekranı", "KartOku", null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "Action", "Baslik", "Controller" },
                values: new object[] { "AidatRapor", "Öğrenci Aidat Raporu", "OgrenciAidat" });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "Action", "Baslik", "Controller" },
                values: new object[] { "YemekRapor", "Öğrenci Yemek Raporu", "OgrenciYemekhane" });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { null, null, "KartOku", null, 8 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { "Index", 22, "Kart Okuma Ekranı", "KartOku", 1 });
        }
    }
}
