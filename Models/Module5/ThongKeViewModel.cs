// Họ và tên: [TÊN CỦA TÔI]
// Mã sinh viên: [MSSV CỦA TÔI]
// Nội dung thực hiện: Module 5 - Lịch sử tham dự, Dashboard, thống kê LINQ, báo cáo.

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;

/// <summary>
/// ViewModel tổng hợp toàn bộ dữ liệu thống kê.
/// Tất cả dữ liệu được tính bằng LINQ trong ThongKeController.
/// </summary>
public class ThongKeViewModel
{
    // === BỘ LỌC ===
    public int? ThangLoc { get; set; }
    public int? NamLoc { get; set; }
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }

    // === 1. THỐNG KÊ ĐĂNG KÝ ===
    public List<ThongKeDangKyTheoSuKien> DangKyTheoSuKien { get; set; } = [];
    public List<ThongKeDangKyTheoLoai> DangKyTheoLoaiSuKien { get; set; } = [];
    public List<ThongKeDangKyTheoDiaDiem> DangKyTheoDiaDiem { get; set; } = [];

    // === 2. THỐNG KÊ DUYỆT ===
    public List<ThongKeDuyetTheoSuKien> DuyetTheoSuKien { get; set; } = [];

    // === 3. THỐNG KÊ CHECK-IN ===
    public List<ThongKeCheckInTheoSuKien> CheckInTheoSuKien { get; set; } = [];
    public List<ThongKeThamGiaPhien> ThamGiaTheoPhien { get; set; } = [];

    // === 4. THỐNG KÊ THEO THỜI GIAN ===
    public List<ThongKeTheoThang> SuKienTheoThang { get; set; } = [];
    public List<ThongKeTheoThang> DangKyTheoThang { get; set; } = [];
    public List<ThongKeTheoThang> CheckInTheoThang { get; set; } = [];

    // === 6. TỶ LỆ ===
    public List<ThongKeTyLeSuKien> TyLeTheoSuKien { get; set; } = [];

    // === TỔNG HỢP ===
    public int TongSuKien { get; set; }
    public int TongDangKy { get; set; }
    public int TongCheckIn { get; set; }
    public double TyLeCheckInTrungBinh { get; set; }
    public double TyLeHuyTrungBinh { get; set; }

    // === THỐNG KÊ NỔI BẬT (Bước 4) ===
    public NoiBatItem? SuKienNhieuDangKyNhat { get; set; }
    public NoiBatItem? SuKienTyLeThamDuCaoNhat { get; set; }
    public NoiBatItem? LoaiSuKienNhieuThamDuNhat { get; set; }
    public NoiBatItem? DiaDiemSuDungNhieuNhat { get; set; }
    public NoiBatItem? PhienNhieuNguoiNhat { get; set; }
    public NoiBatItem? NguoiThamDuNhieuNhat { get; set; }
}

// =====================================================
// 1. THỐNG KÊ ĐĂNG KÝ
// =====================================================

/// <summary>Số đăng ký theo sự kiện — GroupBy MaSuKien</summary>
public class ThongKeDangKyTheoSuKien
{
    public string MaSuKien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public int SoDangKy { get; set; }
}

/// <summary>Số đăng ký theo loại sự kiện — GroupBy MaLoaiSuKien</summary>
public class ThongKeDangKyTheoLoai
{
    public string TenLoaiSuKien { get; set; } = string.Empty;
    public int SoDangKy { get; set; }
    public int SoSuKien { get; set; }
    public double TrungBinhDangKy { get; set; }
}

/// <summary>Số đăng ký theo địa điểm — GroupBy MaDiaDiem</summary>
public class ThongKeDangKyTheoDiaDiem
{
    public string TenDiaDiem { get; set; } = string.Empty;
    public int SoDangKy { get; set; }
    public int SoSuKien { get; set; }
}

// =====================================================
// 2. THỐNG KÊ DUYỆT
// =====================================================

/// <summary>Số người được duyệt theo sự kiện</summary>
public class ThongKeDuyetTheoSuKien
{
    public string TenSuKien { get; set; } = string.Empty;
    public int TongDangKy { get; set; }
    public int SoDaDuyet { get; set; }
    public double TyLeDuyet { get; set; }
}

// =====================================================
// 3. THỐNG KÊ CHECK-IN
// =====================================================

/// <summary>Số check-in thực tế theo sự kiện</summary>
public class ThongKeCheckInTheoSuKien
{
    public string TenSuKien { get; set; } = string.Empty;
    public int SoDaDuyet { get; set; }
    public int SoCheckIn { get; set; }
    public double TyLeCheckIn { get; set; } // AttendanceRate = CheckIn/Approved*100
}

/// <summary>Số người tham gia theo phiên</summary>
public class ThongKeThamGiaPhien
{
    public string TenPhien { get; set; } = string.Empty;
    public string TenSuKien { get; set; } = string.Empty;
    public int SoDangKy { get; set; }
}

// =====================================================
// 4. THỐNG KÊ THEO THỜI GIAN
// =====================================================

/// <summary>Thống kê theo tháng (dùng chung cho SK, ĐK, CI)</summary>
public class ThongKeTheoThang
{
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string NhanThang => $"{Thang:D2}/{Nam}";
    public int SoLuong { get; set; }
}

// =====================================================
// 6. TỶ LỆ
// =====================================================

/// <summary>
/// Tỷ lệ tổng hợp cho từng sự kiện.
/// Xử lý mẫu số = 0 trong Controller.
/// </summary>
public class ThongKeTyLeSuKien
{
    public string TenSuKien { get; set; } = string.Empty;
    public int TongDangKy { get; set; }
    public int SoDaDuyet { get; set; }
    public int SoCheckIn { get; set; }
    public int SoDaHuy { get; set; }
    public int SucChua { get; set; }

    // Attendance Rate = CheckIn / Approved * 100
    public double TyLeCheckIn { get; set; }
    // Cancellation Rate = Canceled / Total * 100
    public double TyLeHuy { get; set; }
    // Fill Rate = Approved / MaxCapacity * 100
    public double TyLeLapDay { get; set; }
}

// =====================================================
// THỐNG KÊ NỔI BẬT (Bước 4)
// =====================================================

/// <summary>
/// Item nổi bật dùng chung cho 6 thống kê highlight.
/// Nullable — nếu không có dữ liệu thì property = null.
/// </summary>
public class NoiBatItem
{
    public string Ten { get; set; } = string.Empty;
    public int GiaTri { get; set; }
    public string ChiTiet { get; set; } = string.Empty;
    public double TyLe { get; set; }
}
