// Họ và tên: PHAM TRUNG DUC
// Mã sinh viên: 23103100134
// Nội dung thực hiện: Module 5 - Xuất báo cáo PDF (nâng cao).

using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Services;

/// <summary>
/// Tạo file PDF cho báo cáo theo thời gian.
/// Dùng QuestPDF: dữ liệu lấy từ BaoCaoViewModel đã tính sẵn trong controller.
/// </summary>
public static class BaoCaoPdfService
{
    public static byte[] TaoPdf(BaoCaoViewModel vm)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                // Font hệ thống (Windows: Arial) hỗ trợ tiếng Việt.
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                // Tiêu đề đầu trang
                page.Header()
                    .Text("BÁO CÁO THỐNG KÊ SỰ KIỆN")
                    .FontSize(16)
                    .Bold();

                page.Content().Column(col =>
                {
                    col.Spacing(6);

                    col.Item().Text($"Khoảng thời gian: {vm.MoTaKhoangThoiGian}").SemiBold();
                    col.Item().Text($"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}");

                    col.Item()
                        .PaddingTop(4)
                        .Text($"Tổng sự kiện: {vm.TongSuKien}    Tổng đăng ký: {vm.TongDangKy}    Tổng check-in: {vm.TongCheckIn}");

                    // 1. Bảng số liệu theo tháng
                    col.Item().PaddingTop(12).Text("1. Số liệu theo tháng").Bold();

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Tháng").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Sự kiện").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Đăng ký").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Check-in").SemiBold();
                        });

                        foreach (var m in vm.SuKienTheoThang)
                        {
                            int sk = vm.SuKienTheoThang.Count == 0 ? 0 : m.SoLuong;
                            int dk = vm.DangKyTheoThang.FirstOrDefault(x => x.Nam == m.Nam && x.Thang == m.Thang)?.SoLuong ?? 0;
                            int ci = vm.CheckInTheoThang.FirstOrDefault(x => x.Nam == m.Nam && x.Thang == m.Thang)?.SoLuong ?? 0;

                            table.Cell().Padding(4).Text(m.NhanThang);
                            table.Cell().Padding(4).Text(sk.ToString());
                            table.Cell().Padding(4).Text(dk.ToString());
                            table.Cell().Padding(4).Text(ci.ToString());
                        }
                    });

                    // 2. Bảng chi tiết sự kiện
                    col.Item().PaddingTop(16).Text("2. Chi tiết sự kiện trong khoảng thời gian").Bold();

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Tên sự kiện").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Thời gian bắt đầu").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Trạng thái").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Đăng ký").SemiBold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Check-in").SemiBold();
                        });

                        foreach (var sk in vm.DanhSachSuKien)
                        {
                            table.Cell().Padding(4).Text(sk.TenSuKien);
                            table.Cell().Padding(4).Text(sk.ThoiGianBatDau.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().Padding(4).Text(TenTrangThai(sk.TrangThai));
                            table.Cell().Padding(4).Text(sk.SoDangKy.ToString());
                            table.Cell().Padding(4).Text(sk.SoCheckIn.ToString());
                        }
                    });
                });

                // Chân trang: số trang
                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                    });
            });
        }).GeneratePdf();
    }

    private static string TenTrangThai(string trangThai) => trangThai switch
    {
        "HoanThanh" => "Hoàn thành",
        "DangDienRa" => "Đang diễn ra",
        "MoDangKy" => "Mở đăng ký",
        "DongDangKy" => "Đóng đăng ký",
        "DaHuy" => "Đã hủy",
        "SapDienRa" => "Nháp",
        _ => trangThai
    };
}