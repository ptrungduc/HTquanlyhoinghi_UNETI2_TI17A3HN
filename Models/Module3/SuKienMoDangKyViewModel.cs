using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module3;

public class SuKienMoDangKyViewModel
{
    public string? TuKhoa { get; set; }
    public string? MaLoaiSuKien { get; set; }
    public IReadOnlyList<SuKienMoDangKyDongViewModel> SuKiens { get; set; } = [];
    public IReadOnlyList<LoaiSuKienLuaChonViewModel> LoaiSuKiens { get; set; } = [];
}

public class SuKienMoDangKyDongViewModel
{
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public string TenLoaiSuKien { get; set; } = string.Empty;
    public string TenDiaDiem { get; set; } = string.Empty;
    public int SucChua { get; set; }
    public int SoLuongHieuLuc { get; set; }
    public DateTime BatDau { get; set; }
}

public class LoaiSuKienLuaChonViewModel
{
    public string MaLoaiSuKien { get; set; } = string.Empty;
    public string TenLoaiSuKien { get; set; } = string.Empty;
}
