// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 5 - Lịch sử tham dự, Dashboard, thống kê LINQ, báo cáo.

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;

/// <summary>
/// ViewModel tổng hợp dữ liệu Dashboard cho Admin/Organizer.
/// Tất cả dữ liệu được tính bằng LINQ trong Controller.
/// </summary>
public class DashboardViewModel
{
    // === 1. TỔNG SỐ SỰ KIỆN ===
    public int TongSuKien { get; set; }

    // === 2. SỐ SỰ KIỆN THEO TRẠNG THÁI ===
    public int SuKienNhap { get; set; }
    public int SuKienMoDangKy { get; set; }
    public int SuKienDongDangKy { get; set; }
    public int SuKienDangDienRa { get; set; }
    public int SuKienHoanThanh { get; set; }
    public int SuKienDaHuy { get; set; }

    // === 3. SỐ ĐĂNG KÝ THEO TRẠNG THÁI ===
    public int DangKyChoDuyet { get; set; }
    public int DangKyDaDuyet { get; set; }
    public int DangKyDaCheckIn { get; set; }
    public int DangKyHoanThanh { get; set; }
    public int DangKyTuChoi { get; set; }
    public int DangKyDaHuy { get; set; }
    public int TongDangKy { get; set; }

    // === 4. SỐ LƯỢT CHECK-IN TRONG NGÀY ===
    public int CheckInHomNay { get; set; }

    // === 5. DANH SÁCH SỰ KIỆN SẮP DIỄN RA ===
    public List<SuKienSapDienRaItem> SuKienSapDienRa { get; set; } = [];

    // === 6. SỐ NGƯỜI THAM DỰ THỰC TẾ THEO THÁNG (12 tháng gần nhất) ===
    public List<ThongKeThamDuTheoThang> ThamDuTheoThang { get; set; } = [];

    // === 7. DANH SÁCH SỰ KIỆN GẦN HẠN ĐĂNG KÝ ===
    public List<SuKienGanHanItem> SuKienGanHan { get; set; } = [];
}

/// <summary>Một sự kiện sắp diễn ra (mục 5).</summary>
public class SuKienSapDienRaItem
{
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public DateTime ThoiGianBatDau { get; set; }
    public string TenDiaDiem { get; set; } = string.Empty;
    public int SoDangKy { get; set; }
    public string TrangThai { get; set; } = string.Empty;
}

/// <summary>Thống kê tham dự theo tháng (mục 6).</summary>
public class ThongKeThamDuTheoThang
{
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string NhanThang => $"{Thang:D2}/{Nam}";
    public int SoLuotCheckIn { get; set; }
}

/// <summary>
/// Sự kiện gần hạn đăng ký (mục 7).
/// Vì SuKien chưa có HanDangKy, dùng ThoiGianBatDau thay thế.
/// </summary>
public class SuKienGanHanItem
{
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public DateTime ThoiGianBatDau { get; set; }
    public int SoNgayConLai { get; set; }
    public int SoDangKy { get; set; }
}

