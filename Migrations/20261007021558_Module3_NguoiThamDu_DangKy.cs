using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyHoiNghi.Migrations
{
    /// <inheritdoc />
    public partial class Module3_NguoiThamDu_DangKy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NguoiThamDus",
                columns: table => new
                {
                    MaNguoiThamDu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonViCongTac = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiThamDus", x => x.MaNguoiThamDu);
                });

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
                name: "DangKyThamDus",
                columns: table => new
                {
                    MaDangKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiThamDu = table.Column<int>(type: "int", nullable: false),
                    MaSuKien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LyDoTuChoiHuy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyThamDus", x => x.MaDangKy);
                    table.ForeignKey(
                        name: "FK_DangKyThamDus_NguoiThamDus_MaNguoiThamDu",
                        column: x => x.MaNguoiThamDu,
                        principalTable: "NguoiThamDus",
                        principalColumn: "MaNguoiThamDu",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DangKyThamDus_SuKiens_MaSuKien",
                        column: x => x.MaSuKien,
                        principalTable: "SuKiens",
                        principalColumn: "MaSuKien",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "DangKyPhiens",
                columns: table => new
                {
                    MaDangKyPhien = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiThamDu = table.Column<int>(type: "int", nullable: false),
                    MaPhien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyPhiens", x => x.MaDangKyPhien);
                    table.ForeignKey(
                        name: "FK_DangKyPhiens_NguoiThamDus_MaNguoiThamDu",
                        column: x => x.MaNguoiThamDu,
                        principalTable: "NguoiThamDus",
                        principalColumn: "MaNguoiThamDu",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DangKyPhiens_PhienSuKiens_MaPhien",
                        column: x => x.MaPhien,
                        principalTable: "PhienSuKiens",
                        principalColumn: "MaPhienSuKien",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DangKyPhiens_MaNguoiThamDu",
                table: "DangKyPhiens",
                column: "MaNguoiThamDu");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyPhiens_MaPhien",
                table: "DangKyPhiens",
                column: "MaPhien");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyThamDus_MaNguoiThamDu",
                table: "DangKyThamDus",
                column: "MaNguoiThamDu");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyThamDus_MaSuKien",
                table: "DangKyThamDus",
                column: "MaSuKien");

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
                column: "MaLoaiSuKien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DangKyPhiens");

            migrationBuilder.DropTable(
                name: "DangKyThamDus");

            migrationBuilder.DropTable(
                name: "PhienSuKiens");

            migrationBuilder.DropTable(
                name: "NguoiThamDus");

            migrationBuilder.DropTable(
                name: "SuKiens");
        }
    }
}
