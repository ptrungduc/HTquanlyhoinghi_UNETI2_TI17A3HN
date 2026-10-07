namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;

public class DiemDanhPhienViewModel
{
    public List<PhienDiemDanhLuaChonViewModel> DanhSachPhien { get; set; } = [];
    public List<NguoiDiemDanhPhienViewModel> DanhSachNguoiThamDu { get; set; } = [];
    public string? MaPhienSuKien { get; set; }
    public string TenPhien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public string ThongBao { get; set; } = string.Empty;
    public KetQuaThaoTacViewModel? KetQua { get; set; }
}

public class PhienDiemDanhLuaChonViewModel
{
    public string MaPhienSuKien { get; set; } = string.Empty;
    public string TenHienThi { get; set; } = string.Empty;
}

public class NguoiDiemDanhPhienViewModel
{
    public string MaNguoiThamDu { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public TrangThaiDangKy TrangThaiDangKyPhien { get; set; }
    public bool DaDiemDanh { get; set; }
}