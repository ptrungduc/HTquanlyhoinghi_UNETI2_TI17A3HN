using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyHoiNghi.Migrations;

public partial class Module4AttendanceMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "KetQuaDiemDanh",
            table: "DangKyPhiens",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "NguoiDiemDanh",
            table: "DangKyPhiens",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "ThoiGianDiemDanh",
            table: "DangKyPhiens",
            type: "datetime2",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_DangKyThamDus_MaThamDu",
            table: "DangKyThamDus",
            column: "MaThamDu",
            unique: true,
            filter: "[MaThamDu] IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_DangKyThamDus_MaThamDu",
            table: "DangKyThamDus");

        migrationBuilder.DropColumn(
            name: "KetQuaDiemDanh",
            table: "DangKyPhiens");

        migrationBuilder.DropColumn(
            name: "NguoiDiemDanh",
            table: "DangKyPhiens");

        migrationBuilder.DropColumn(
            name: "ThoiGianDiemDanh",
            table: "DangKyPhiens");
    }
}
