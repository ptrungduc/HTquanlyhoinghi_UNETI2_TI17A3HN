using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module3;

public class DangKyDanhSachViewModel
{
    public IReadOnlyList<DangKyDongViewModel> DangKys { get; set; } = [];
    public string? TrangThai { get; set; }
    public string? MaLoaiSuKien { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }
    public string SapXep { get; set; } = "ngaydangky";
    public IReadOnlyList<LoaiSuKien> LoaiSuKiens { get; set; } = [];
}

public class DangKyDongViewModel
{
    public string MaDangKy { get; set; } = string.Empty;
    public string? MaNguoiThamDu { get; set; }
    public string TenNguoiThamDu { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public string MaLoaiSuKien { get; set; } = string.Empty;
    public DateTime NgayDangKy { get; set; }
    public DateTime ThoiGianBatDau { get; set; }
    public TrangThaiDangKy TrangThai { get; set; }
    public DateTime HanHuy { get; set; }

    public string TrangThaiHienThi => TrangThai switch
    {
        TrangThaiDangKy.ChoDuyet => "Chờ duyệt",
        TrangThaiDangKy.DaDuyet => "Đã duyệt",
        TrangThaiDangKy.DaCheckIn => "Đã check-in",
        TrangThaiDangKy.HoanThanh => "Hoàn thành",
        TrangThaiDangKy.TuChoi => "Từ chối",
        TrangThaiDangKy.DaHuy => "Đã hủy",
        _ => TrangThai.ToString()
    };
}

public class DangKyTimKiemViewModel : DangKyDanhSachViewModel
{
    public string? TenNguoiThamDu { get; set; }
    public string? MaSuKien { get; set; }
}
