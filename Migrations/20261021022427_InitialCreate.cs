using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyHoiNghi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiaDiems",
                columns: table => new
                {
                    MaDiaDiem = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenDiaDiem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SucChuaToiDa = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaDiems", x => x.MaDiaDiem);
                });

            migrationBuilder.CreateTable(
                name: "LoaiSuKiens",
                columns: table => new
                {
                    MaLoaiSuKien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenLoaiSuKien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiSuKiens", x => x.MaLoaiSuKien);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.MaTaiKhoan);
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
                name: "NguoiThamDus",
                columns: table => new
                {
                    MaNguoiThamDu = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaTaiKhoan = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonViCongTac = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiThamDus", x => x.MaNguoiThamDu);
                    table.ForeignKey(
                        name: "FK_NguoiThamDus_TaiKhoans_MaTaiKhoan",
                        column: x => x.MaTaiKhoan,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTaiKhoan",
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

            migrationBuilder.CreateTable(
                name: "DangKyThamDus",
                columns: table => new
                {
                    MaDangKy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaNguoiThamDu = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaSuKien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LyDoTuChoiHuy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaThamDu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyThamDus", x => x.MaDangKy);
                    table.ForeignKey(
                        name: "FK_DangKyThamDus_NguoiThamDus_MaNguoiThamDu",
                        column: x => x.MaNguoiThamDu,
                        principalTable: "NguoiThamDus",
                        principalColumn: "MaNguoiThamDu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKyThamDus_SuKiens_MaSuKien",
                        column: x => x.MaSuKien,
                        principalTable: "SuKiens",
                        principalColumn: "MaSuKien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DangKyPhiens",
                columns: table => new
                {
                    MaDangKyPhien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaNguoiThamDu = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKyPhiens_PhienSuKiens_MaPhien",
                        column: x => x.MaPhien,
                        principalTable: "PhienSuKiens",
                        principalColumn: "MaPhienSuKien",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    MaCheckIn = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaDangKy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ThoiGianCheckIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiThucHien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.MaCheckIn);
                    table.ForeignKey(
                        name: "FK_CheckIns_DangKyThamDus_MaDangKy",
                        column: x => x.MaDangKy,
                        principalTable: "DangKyThamDus",
                        principalColumn: "MaDangKy",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_MaDangKy",
                table: "CheckIns",
                column: "MaDangKy",
                unique: true);

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
                name: "IX_LoaiSuKiens_TenLoaiSuKien",
                table: "LoaiSuKiens",
                column: "TenLoaiSuKien",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiThamDus_MaTaiKhoan",
                table: "NguoiThamDus",
                column: "MaTaiKhoan");

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

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_TenDangNhap",
                table: "TaiKhoans",
                column: "TenDangNhap",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckIns");

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

            migrationBuilder.DropTable(
                name: "TaiKhoans");

            migrationBuilder.DropTable(
                name: "DiaDiems");

            migrationBuilder.DropTable(
                name: "LoaiSuKiens");
        }
    }
}
