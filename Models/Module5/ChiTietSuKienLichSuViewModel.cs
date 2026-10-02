// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 5 - Lịch sử tham dự, Dashboard, thống kê LINQ, báo cáo.

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;

/// <summary>
/// ViewModel hiển thị chi tiết lịch sử một sự kiện cho Admin.
/// Dùng cho action LichSu/ChiTietSuKien.
/// </summary>
public class ChiTietSuKienLichSuViewModel
{
    // --- Thông tin sự kiện ---
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public string TenLoaiSuKien { get; set; } = string.Empty;
    public string TenDiaDiem { get; set; } = string.Empty;
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime ThoiGianKetThuc { get; set; }
    public string TrangThaiSuKien { get; set; } = string.Empty;

    // --- Thống kê tổng hợp (tính bằng LINQ trong Controller) ---
    public int TongDangKy { get; set; }
    public int SoDaDuyet { get; set; }
    public int SoDaCheckIn { get; set; }
    public int SoTuChoi { get; set; }
    public int SoDaHuy { get; set; }

    // --- Danh sách ---
    public List<DangKyThamDuItem> DanhSachDangKy { get; set; } = [];
    public List<PhienSuKienItem> DanhSachPhien { get; set; } = [];
}

/// <summary>
/// Một dòng trong danh sách đăng ký tham dự của sự kiện (phía Admin).
/// </summary>
public class DangKyThamDuItem
{
    public string MaDangKy { get; set; } = string.Empty;
    public string HoTenNguoiThamDu { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime NgayDangKy { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? NgayDuyet { get; set; }
    public DateTime? ThoiGianCheckIn { get; set; }
    public string? MaThamDu { get; set; }
    public string KetQuaThamGia { get; set; } = string.Empty;
}

/// <summary>
/// Một dòng trong danh sách phiên sự kiện.
/// </summary>
public class PhienSuKienItem
{
    public string MaPhienSuKien { get; set; } = string.Empty;
    public string TenPhien { get; set; } = string.Empty;
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime ThoiGianKetThuc { get; set; }
    public string DienGia { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
}
