namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module3;

public class DangKyModule3ViewModel
{
    public string Tab { get; set; } = "su-kien";
    public bool LaQuanTri { get; set; }
    public bool CoHoSoNguoiThamDu { get; set; }
    public string? TuKhoaSuKien { get; set; }
    public string? MaLoaiSuKienSuKien { get; set; }
    public string? TenNguoiThamDu { get; set; }
    public string? MaSuKien { get; set; }
    public string? TrangThai { get; set; }
    public string? MaLoaiSuKien { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public string SapXep { get; set; } = "ngaydangky";
    public IReadOnlyList<LoaiSuKienLuaChonViewModel> LoaiSuKiens { get; set; } = [];
    public IReadOnlyList<SuKienMoDangKyDongViewModel> SuKiens { get; set; } = [];
    public IReadOnlyList<DangKyDongViewModel> DangKysCuaToi { get; set; } = [];
    public IReadOnlyList<DangKyDongViewModel> KetQuaTimKiem { get; set; } = [];
}
