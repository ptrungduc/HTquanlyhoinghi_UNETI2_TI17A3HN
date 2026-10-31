// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 4 - Xét duyệt đăng ký, mã tham dự, check-in, điểm danh.

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;

public class DuyetDangKyViewModel
{
    public List<DangKyChoDuyetViewModel> DanhSachDangKy { get; set; } = [];
    public List<SuKienLuaChonViewModel> DanhSachSuKien { get; set; } = [];
    public string? MaSuKien { get; set; }
    public TrangThaiDangKy? TrangThai { get; set; }
    public string? TuKhoa { get; set; }
    public string ThongBao { get; set; } = string.Empty;
}

public class DangKyChoDuyetViewModel
{
    public string MaDangKy { get; set; } = string.Empty;
    public string? MaSuKien { get; set; }
    public string TenNguoiThamDu { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public string? MaThamDu { get; set; }
}

public class SuKienLuaChonViewModel
{
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
}

public class DangKyPhienChoDuyetViewModel
{
    public List<PhienDiemDanhLuaChonViewModel> DanhSachPhien { get; set; } = [];
    public List<DangKyPhienChoDuyetItemViewModel> DanhSachDangKy { get; set; } = [];
    public string? MaPhienSuKien { get; set; }
    public string ThongBao { get; set; } = string.Empty;
    public KetQuaThaoTacViewModel? KetQua { get; set; }
}

public class DangKyPhienChoDuyetItemViewModel
{
    public string MaDangKyPhien { get; set; } = string.Empty;
    public string MaNguoiThamDu { get; set; } = string.Empty;
    public string TenNguoiThamDu { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public string TenPhien { get; set; } = string.Empty;
    public DateTime NgayDangKy { get; set; }
    public string TrangThai { get; set; } = string.Empty;
}