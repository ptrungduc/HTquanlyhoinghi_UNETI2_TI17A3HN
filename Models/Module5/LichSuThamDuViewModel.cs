// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 5 - Lịch sử tham dự, Dashboard, thống kê LINQ, báo cáo.

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;

/// <summary>
/// ViewModel hiển thị lịch sử tham dự của một người tham dự.
/// Dùng cho action LichSu/CaNhan.
/// </summary>
public class LichSuThamDuViewModel
{
    public string HoTenNguoiThamDu { get; set; } = string.Empty;
    public List<LichSuDangKyItem> DanhSachDangKy { get; set; } = [];
}

/// <summary>
/// Một dòng trong danh sách lịch sử đăng ký của người tham dự.
/// </summary>
public class LichSuDangKyItem
{
    public string MaDangKy { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime ThoiGianKetThuc { get; set; }
    public string TenDiaDiem { get; set; } = string.Empty;
    public string DiaChi { get; set; } = string.Empty;
    public DateTime NgayDangKy { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? ThoiGianCheckIn { get; set; }
    public string? MaThamDu { get; set; }
}
