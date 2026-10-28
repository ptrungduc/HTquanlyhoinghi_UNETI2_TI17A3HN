// Họ và tên: PHAM TRUNG DUC
// Mã sinh viên: 23103100134
// Nội dung thực hiện: Module 5 - Dashboard Organizer/Admin (Bước 2).

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;

/// <summary>
/// ViewModel cho trang Dashboard quản trị.
/// Toàn bộ số liệu được tính từ database bằng EF Core + LINQ trong DashboardController.
/// </summary>
public class DashboardViewModel
{
    // 1. Tổng số sự kiện
    public int TongSuKien { get; set; }

    // 2. Số sự kiện theo trạng thái (đủ 6 trạng thái, kể cả = 0)
    public List<DashboardThongKeItem> SuKienTheoTrangThai { get; set; } = [];

    // 3. Số đăng ký theo trạng thái (đủ 6 trạng thái, kể cả = 0)
    public List<DashboardThongKeItem> DangKyTheoTrangThai { get; set; } = [];

    // 4. Số lượt check-in trong ngày hôm nay
    public int CheckInHomNay { get; set; }

    // 5. Danh sách sự kiện sắp diễn ra
    public List<DashboardSuKienItem> SuKienSapDienRa { get; set; } = [];

    // 6. Số người tham dự thực tế (check-in) theo tháng, 12 tháng gần nhất
    public List<DashboardThangItem> NguoiThamDuTheoThang { get; set; } = [];

    // 7. Danh sách sự kiện gần hạn đăng ký
    public List<DashboardSuKienItem> SuKienGanHanDangKy { get; set; } = [];

    /// <summary>Thời điểm tính dữ liệu, hiển thị trên trang.</summary>
    public DateTime ThoiDiemTinh { get; set; }
}

/// <summary>Một dòng thống kê theo trạng thái.</summary>
public class DashboardThongKeItem
{
    public string MaTrangThai { get; set; } = string.Empty;
    public string TenHienThi { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public int SoLuong { get; set; }
}

/// <summary>Một sự kiện hiển thị trong danh sách trên Dashboard.</summary>
public class DashboardSuKienItem
{
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public string TenDiaDiem { get; set; } = string.Empty;
    public DateTime ThoiGianBatDau { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int SoDangKyHieuLuc { get; set; }
    public int SucChua { get; set; }
}

/// <summary>Số người tham dự thực tế (check-in) của một tháng.</summary>
public class DashboardThangItem
{
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string NhanThang => $"{Thang:D2}/{Nam}";
    public int SoNguoi { get; set; }
}
