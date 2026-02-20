using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class ziyaretciler3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Not",
                table: "Ziyaretciler");

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 9,
                column: "Sirala",
                value: 4);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 11,
                column: "Sirala",
                value: 5);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 13,
                column: "Sirala",
                value: 6);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 16,
                column: "Sirala",
                value: 7);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 18,
                column: "Sirala",
                value: 8);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Action", "Baslik", "Controller" },
                values: new object[] { "ZiyaretciRapor", "Öğrenci Ziyaretçi Raporu", "Ziyaretci" });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { "YemekRapor", 18, "Öğrenci Yemek Raporu", "OgrenciYemekhane", 5 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { null, null, "KartOku", null, 9 });

            migrationBuilder.InsertData(
                table: "MenuOgeler",
                columns: new[] { "Id", "Action", "AnaMenuId", "Baslik", "Controller", "GerekliRole", "Sirala" },
                values: new object[] { 25, "Index", 24, "Kart Okuma Ekranı", "KartOku", null, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.AddColumn<string>(
                name: "Not",
                table: "Ziyaretciler",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 9,
                column: "Sirala",
                value: 8);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 11,
                column: "Sirala",
                value: 4);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 13,
                column: "Sirala",
                value: 5);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 16,
                column: "Sirala",
                value: 6);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 18,
                column: "Sirala",
                value: 7);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "Action", "Baslik", "Controller" },
                values: new object[] { "YemekRapor", "Öğrenci Yemek Raporu", "OgrenciYemekhane" });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { null, null, "KartOku", null, 8 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { "Index", 23, "Kart Okuma Ekranı", "KartOku", 1 });
        }
    }
}
