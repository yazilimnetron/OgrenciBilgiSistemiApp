using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class ziyaretciler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ziyaretciler",
                columns: table => new
                {
                    ZiyaretciId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TcKimlikNo = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PersonelId = table.Column<int>(type: "int", nullable: true),
                    ZiyaretSebebi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    KartNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    KartVerildiMi = table.Column<bool>(type: "bit", nullable: false),
                    GirisZamani = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CikisZamani = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    Not = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CihazId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ziyaretciler", x => x.ZiyaretciId);
                    table.ForeignKey(
                        name: "FK_Ziyaretciler_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Personeller",
                        principalColumn: "PersonelId");
                });

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 20,
                column: "Controller",
                value: "Ogrenciler");

            migrationBuilder.CreateIndex(
                name: "IX_Ziyaretciler_PersonelId",
                table: "Ziyaretciler",
                column: "PersonelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ziyaretciler");

            migrationBuilder.UpdateData(
                table: "MenuOgeler",
                keyColumn: "Id",
                keyValue: 20,
                column: "Controller",
                value: "Ogrenci");
        }
    }
}
