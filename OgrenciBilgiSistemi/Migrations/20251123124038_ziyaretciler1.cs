using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class ziyaretciler1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Baslik", "Controller", "Sirala" },
                values: new object[] { "Aidat İşlemleri", "OgrenciAidat", 2 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Baslik", "Controller", "Sirala" },
                values: new object[] { "Yemekhane İşlemleri", "OgrenciYemekhane", 3 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { null, null, "Ziyaretçiler", null, 8 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { "Index", 9, "Ziyaretçi İşlemleri", "Ziyaretci", 1 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 12,
                column: "AnaMenuId",
                value: 11);

            migrationBuilder.InsertData(
                table: "MenuOgeler",
                columns: new[] { "Id", "Action", "AnaMenuId", "Baslik", "Controller", "GerekliRole", "Sirala" },
                values: new object[,]
                {
                    { 6, "Index", 5, "Öğrenci İşlemleri", "Ogrenciler", null, 1 },
                    { 11, null, null, "Kullanıcılar", null, null, 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Baslik", "Controller", "Sirala" },
                values: new object[] { "Öğrenci İşlemleri", "Ogrenciler", 1 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Baslik", "Controller", "Sirala" },
                values: new object[] { "Aidat İşlemleri", "OgrenciAidat", 2 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { "Index", 5, "Yemekhane İşlemleri", "OgrenciYemekhane", 3 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Action", "AnaMenuId", "Baslik", "Controller", "Sirala" },
                values: new object[] { null, null, "Kullanıcılar", null, 4 });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 12,
                column: "AnaMenuId",
                value: 10);
        }
    }
}
