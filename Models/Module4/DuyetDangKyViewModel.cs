// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;

public class DuyetDangKyViewModel
{
    public List<DangKyChoDuyetViewModel> DanhSachDangKy { get; set; } = [];
    public string ThongBaoTichHop { get; set; } = string.Empty;
}

public class DangKyChoDuyetViewModel
{
    public string MaDangKy { get; set; } = string.Empty;
    public string TenNguoiThamDu { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
}