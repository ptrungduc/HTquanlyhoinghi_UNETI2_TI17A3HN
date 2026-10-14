using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module3;

public class DangKyFormViewModel
{
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public string TenLoaiSuKien { get; set; } = string.Empty;
    public string TenDiaDiem { get; set; } = string.Empty;
    public int SucChuaDiaDiem { get; set; }
    public DateTime ThoiGianBatDau { get; set; }
    public DateTime ThoiGianKetThuc { get; set; }
    public string TrangThaiSuKien { get; set; } = string.Empty;
    public int SoLuongDangKyHieuLuc { get; set; }
    public bool CoTheDangKy { get; set; }
    public string? LyDoKhongTheDangKy { get; set; }
}
