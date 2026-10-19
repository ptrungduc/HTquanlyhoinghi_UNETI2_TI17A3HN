using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyHoiNghi.Migrations
{
    public partial class Module2_EventScheduleRules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HanDangKy",
                table: "SuKiens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoLuongToiDa",
                table: "SuKiens",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhongKhuVuc",
                table: "PhienSuKiens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoLuongToiDa",
                table: "PhienSuKiens",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE s
                SET HanDangKy = s.ThoiGianBatDau,
                    SoLuongToiDa = CASE
                        WHEN d.SucChuaToiDa > 0 THEN d.SucChuaToiDa
                        ELSE 1
                    END
                FROM SuKiens AS s
                LEFT JOIN DiaDiems AS d ON d.MaDiaDiem = s.MaDiaDiem;

                UPDATE p
                SET PhongKhuVuc = N'Chưa xác định',
                    SoLuongToiDa = CASE
                        WHEN s.SoLuongToiDa > 0 THEN s.SoLuongToiDa
                        ELSE 1
                    END
                FROM PhienSuKiens AS p
                INNER JOIN SuKiens AS s ON s.MaSuKien = p.MaSuKien;
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "HanDangKy",
                table: "SuKiens",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SoLuongToiDa",
                table: "SuKiens",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhongKhuVuc",
                table: "PhienSuKiens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SoLuongToiDa",
                table: "PhienSuKiens",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_DangKyPhiens_MaNguoiThamDu",
                table: "DangKyPhiens",
                column: "MaNguoiThamDu");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyPhiens_MaPhien",
                table: "DangKyPhiens",
                column: "MaPhien");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DangKyPhiens");

            migrationBuilder.DropColumn(name: "HanDangKy", table: "SuKiens");
            migrationBuilder.DropColumn(name: "SoLuongToiDa", table: "SuKiens");
            migrationBuilder.DropColumn(name: "PhongKhuVuc", table: "PhienSuKiens");
            migrationBuilder.DropColumn(name: "SoLuongToiDa", table: "PhienSuKiens");
        }
    }
}
