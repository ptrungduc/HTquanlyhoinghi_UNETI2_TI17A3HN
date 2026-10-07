using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyHoiNghi.Migrations
{
    /// <inheritdoc />
    public partial class ThemNguoiThamDuDangKyCheckIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "DangKyThamDus",
                columns: table => new
                {
                    MaDangKy = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaNguoiThamDu = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaSuKien = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
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
                name: "IX_DangKyThamDus_MaNguoiThamDu",
                table: "DangKyThamDus",
                column: "MaNguoiThamDu");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyThamDus_MaSuKien",
                table: "DangKyThamDus",
                column: "MaSuKien");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiThamDus_MaTaiKhoan",
                table: "NguoiThamDus",
                column: "MaTaiKhoan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckIns");

            migrationBuilder.DropTable(
                name: "DangKyThamDus");

            migrationBuilder.DropTable(
                name: "NguoiThamDus");
        }
    }
}
