using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OgrenciBilgiSistemi.Migrations
{
    /// <inheritdoc />
    public partial class ogrenciveli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OgrenciVeliAdSoyad",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "OgrenciVeliTelefon",
                table: "Ogrenciler");

            migrationBuilder.AddColumn<int>(
                name: "OgrenciVeliId",
                table: "Ogrenciler",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OgrenciVeliler",
                columns: table => new
                {
                    OgrenciVeliId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VeliAdSoyad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VeliTelefon = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    VeliAdres = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    VeliMeslek = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VeliIsYeri = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VeliEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VeliYakinlik = table.Column<int>(type: "int", nullable: true),
                    VeliDurum = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OgrenciVeliler", x => x.OgrenciVeliId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ogrenciler_OgrenciVeliId",
                table: "Ogrenciler",
                column: "OgrenciVeliId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ogrenciler_OgrenciVeliler_OgrenciVeliId",
                table: "Ogrenciler",
                column: "OgrenciVeliId",
                principalTable: "OgrenciVeliler",
                principalColumn: "OgrenciVeliId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ogrenciler_OgrenciVeliler_OgrenciVeliId",
                table: "Ogrenciler");

            migrationBuilder.DropTable(
                name: "OgrenciVeliler");

            migrationBuilder.DropIndex(
                name: "IX_Ogrenciler_OgrenciVeliId",
                table: "Ogrenciler");

            migrationBuilder.DropColumn(
                name: "OgrenciVeliId",
                table: "Ogrenciler");

            migrationBuilder.AddColumn<string>(
                name: "OgrenciVeliAdSoyad",
                table: "Ogrenciler",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OgrenciVeliTelefon",
                table: "Ogrenciler",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);
        }
    }
}
