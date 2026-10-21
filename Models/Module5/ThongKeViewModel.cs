// Họ và tên: PHAM TRUNG DUC
// Mã sinh viên: 23103100134
// Nội dung thực hiện: Module 5 - Thống kê LINQ và thống kê nổi bật (Bước 3 + Bước 4).

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;

/// <summary>
/// ViewModel tổng hợp dữ liệu thống kê.
/// Mọi số liệu được tính bằng EF Core + LINQ trong ThongKeController.
/// </summary>
public class ThongKeViewModel
{
    // BỘ LỌC
    public int? ThangLoc { get; set; }
    public int? NamLoc { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }

    /// <summary>true khi người dùng đã chọn một bộ lọc hợp lệ.</summary>
    public bool DaLoc { get; set; }

    /// <summary>Mô tả phạm vi đang xem.</summary>
    public string MoTaKhoangThoiGian { get; set; } = "Toàn bộ dữ liệu";

    /// <summary>Lỗi validation. Null nếu hợp lệ.</summary>
    public string? LoiValidation { get; set; }

    /// <summary>true nếu có ít nhất một bản ghi trong phạm vi đã lọc.</summary>
    public bool CoDuLieu => TongSuKien > 0 || TongDangKy > 0 || TongCheckIn > 0;

    // TỔNG HỢP
    public int TongSuKien { get; set; }
    public int TongDangKy { get; set; }
    public int TongCheckIn { get; set; }

    // 1. THỐNG KÊ ĐĂNG KÝ
    public List<ThongKeDangKyTheoSuKien> DangKyTheoSuKien { get; set; } = [];
    public List<ThongKeDangKyTheoLoai> DangKyTheoLoaiSuKien { get; set; } = [];
    public List<ThongKeDangKyTheoDiaDiem> DangKyTheoDiaDiem { get; set; } = [];

    // 2. THỐNG KÊ DUYỆT
    public List<ThongKeDuyetTheoSuKien> DuyetTheoSuKien { get; set; } = [];

    // 3. THỐNG KÊ CHECK-IN
    public List<ThongKeCheckInTheoSuKien> CheckInTheoSuKien { get; set; } = [];
    public List<ThongKeThamGiaPhien> ThamGiaTheoPhien { get; set; } = [];

    // 4. THỐNG KÊ THEO THỜI GIAN
    public List<ThongKeTheoThang> SuKienTheoThang { get; set; } = [];
    public List<ThongKeTheoThang> DangKyTheoThang { get; set; } = [];
    public List<ThongKeTheoThang> CheckInTheoThang { get; set; } = [];

    // 5. TỶ LỆ
    public List<ThongKeTyLeSuKien> TyLeTheoSuKien { get; set; } = [];
    public double TyLeCheckInTrungBinh { get; set; }
    public double TyLeHuyTrungBinh { get; set; }

    // 6. THỐNG KÊ NỔI BẬT (null = chưa có dữ liệu)
    public NoiBatItem? SuKienNhieuDangKyNhat { get; set; }
    public NoiBatItem? SuKienTyLeThamDuCaoNhat { get; set; }
    public NoiBatItem? LoaiSuKienNhieuThamDuNhat { get; set; }
    public NoiBatItem? DiaDiemSuDungNhieuNhat { get; set; }
    public NoiBatItem? PhienNhieuNguoiNhat { get; set; }
    public NoiBatItem? NguoiThamDuNhieuNhat { get; set; }
}

/// <summary>Số đăng ký theo sự kiện. Gom nhóm theo MaSuKien.</summary>
public class ThongKeDangKyTheoSuKien
{
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public int SoDangKy { get; set; }
}

/// <summary>Số đăng ký theo loại sự kiện. Gom nhóm theo MaLoaiSuKien.</summary>
public class ThongKeDangKyTheoLoai
{
    public string MaLoaiSuKien { get; set; } = string.Empty;
    public string TenLoaiSuKien { get; set; } = string.Empty;
    public int SoSuKien { get; set; }
    public int SoDangKy { get; set; }
    public double TrungBinhDangKy { get; set; }
}

/// <summary>Số đăng ký theo địa điểm. Gom nhóm theo MaDiaDiem.</summary>
public class ThongKeDangKyTheoDiaDiem
{
    public string MaDiaDiem { get; set; } = string.Empty;
    public string TenDiaDiem { get; set; } = string.Empty;
    public int SoSuKien { get; set; }
    public int SoDangKy { get; set; }
}

/// <summary>Số người được duyệt theo sự kiện.</summary>
public class ThongKeDuyetTheoSuKien
{
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public int TongDangKy { get; set; }
    public int SoDaDuyet { get; set; }
    public double TyLeDuyet { get; set; }
}

/// <summary>Số check-in thực tế (của đăng ký đã duyệt) theo sự kiện.</summary>
public class ThongKeCheckInTheoSuKien
{
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public int SoDaDuyet { get; set; }
    public int SoCheckIn { get; set; }
    public double TyLeCheckIn { get; set; }
}

/// <summary>Số người tham gia từng phiên. Gom nhóm theo MaPhienSuKien.</summary>
public class ThongKeThamGiaPhien
{
    public string MaPhienSuKien { get; set; } = string.Empty;
    public string TenPhien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public int SoNguoiThamGia { get; set; }

    /// <summary>Alias cho View cũ đang đọc SoDangKy.</summary>
    public int SoDangKy => SoNguoiThamGia;
}

/// <summary>Thống kê theo tháng, dùng chung cho sự kiện, đăng ký và check-in.</summary>
public class ThongKeTheoThang
{
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string NhanThang => $"{Thang:D2}/{Nam}";
    public int SoLuong { get; set; }
}

/// <summary>Tỷ lệ theo sự kiện. Mẫu số = 0 đã được xử lý trong Controller.</summary>
public class ThongKeTyLeSuKien
{
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public int TongDangKy { get; set; }
    public int SoDaDuyet { get; set; }
    public int SoCheckIn { get; set; }
    public int SoDaHuy { get; set; }
    public int SucChua { get; set; }

    /// <summary>Attendance Rate = CheckIn / Approved * 100</summary>
    public double TyLeCheckIn { get; set; }

    /// <summary>Cancellation Rate = Canceled / Total * 100</summary>
    public double TyLeHuy { get; set; }

    /// <summary>Fill Rate = Approved / MaxCapacity * 100</summary>
    public double TyLeLapDay { get; set; }
}

/// <summary>Một mục nổi bật dùng chung cho sáu thống kê.</summary>
public class NoiBatItem
{
    public string Ten { get; set; } = string.Empty;
    public int GiaTri { get; set; }
    public string ChiTiet { get; set; } = string.Empty;
    public double TyLe { get; set; }
}
