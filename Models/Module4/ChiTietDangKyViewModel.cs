// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;

public class ChiTietDangKyViewModel
{
    public string? MaDangKy { get; set; }
    public string TenNguoiThamDu { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SoDienThoai { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? NgayDangKy { get; set; }
    public DateTime? NgayDuyet { get; set; }
    public string? MaThamDu { get; set; }
    public string ThongBao { get; set; } = string.Empty;
}