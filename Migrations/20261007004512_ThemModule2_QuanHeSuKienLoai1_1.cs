using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyHoiNghi.Migrations
{
    /// <inheritdoc />
    public partial class ThemModule2_QuanHeSuKienLoai1_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SuKiens",
                columns: table => new
                {
                    MaSuKien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenSuKien = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaLoaiSuKien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaDiaDiem = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuKiens", x => x.MaSuKien);
                    table.ForeignKey(
                        name: "FK_SuKiens_DiaDiems_MaDiaDiem",
                        column: x => x.MaDiaDiem,
                        principalTable: "DiaDiems",
                        principalColumn: "MaDiaDiem",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuKiens_LoaiSuKiens_MaLoaiSuKien",
                        column: x => x.MaLoaiSuKien,
                        principalTable: "LoaiSuKiens",
                        principalColumn: "MaLoaiSuKien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhienSuKiens",
                columns: table => new
                {
                    MaPhienSuKien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaSuKien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenPhien = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DienGia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhienSuKiens", x => x.MaPhienSuKien);
                    table.ForeignKey(
                        name: "FK_PhienSuKiens_SuKiens_MaSuKien",
                        column: x => x.MaSuKien,
                        principalTable: "SuKiens",
                        principalColumn: "MaSuKien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhienSuKiens_MaSuKien",
                table: "PhienSuKiens",
                column: "MaSuKien");

            migrationBuilder.CreateIndex(
                name: "IX_SuKiens_MaDiaDiem",
                table: "SuKiens",
                column: "MaDiaDiem");

            migrationBuilder.CreateIndex(
                name: "IX_SuKiens_MaLoaiSuKien",
                table: "SuKiens",
                column: "MaLoaiSuKien",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhienSuKiens");

            migrationBuilder.DropTable(
                name: "SuKiens");
        }
    }
}
