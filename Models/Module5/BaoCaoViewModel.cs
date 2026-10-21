// Họ và tên: PHAM TRUNG DUC
// Mã sinh viên: 23103100134
// Nội dung thực hiện: Module 5 - Báo cáo theo thời gian (Mục 9.6).

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module5;

/// <summary>
/// ViewModel cho trang Báo cáo theo thời gian.
/// Hỗ trợ lọc theo tháng/năm HOẶC khoảng ngày.
/// </summary>
public class BaoCaoViewModel
{
    // ==============================
    // BỘ LỌC
    // ==============================

    /// <summary>Tháng lọc (1-12), null = không lọc theo tháng.</summary>
    public int? Thang { get; set; }

    /// <summary>Năm lọc, null = không lọc theo năm.</summary>
    public int? Nam { get; set; }

    /// <summary>Ngày bắt đầu khoảng lọc.</summary>
    public DateTime? TuNgay { get; set; }

    /// <summary>Ngày kết thúc khoảng lọc.</summary>
    public DateTime? DenNgay { get; set; }

    /// <summary>Lỗi validation (VD: TuNgay > DenNgay).</summary>
    public string? LoiValidation { get; set; }

    /// <summary>Mô tả khoảng thời gian đã lọc.</summary>
    public string MoTaKhoangThoiGian { get; set; } = string.Empty;

    /// <summary>true nếu người dùng đã bấm Lọc.</summary>
    public bool DaLoc { get; set; }

    // ==============================
    // KẾT QUẢ TỔNG HỢP
    // ==============================

    /// <summary>Tổng số sự kiện trong khoảng thời gian.</summary>
    public int TongSuKien { get; set; }

    /// <summary>Tổng số đăng ký trong khoảng thời gian.</summary>
    public int TongDangKy { get; set; }

    /// <summary>Tổng số check-in trong khoảng thời gian.</summary>
    public int TongCheckIn { get; set; }

    // ==============================
    // BIỂU ĐỒ THEO THÁNG
    // ==============================

    /// <summary>Số sự kiện theo tháng — GroupBy tháng/năm.</summary>
    public List<BaoCaoTheoThangItem> SuKienTheoThang { get; set; } = [];

    /// <summary>Số đăng ký theo tháng — GroupBy tháng/năm.</summary>
    public List<BaoCaoTheoThangItem> DangKyTheoThang { get; set; } = [];

    /// <summary>Số check-in theo tháng — GroupBy tháng/năm.</summary>
    public List<BaoCaoTheoThangItem> CheckInTheoThang { get; set; } = [];

    // ==============================
    // CHI TIẾT SỰ KIỆN TRONG KHOẢNG
    // ==============================

    /// <summary>Danh sách sự kiện trong khoảng thời gian.</summary>
    public List<BaoCaoSuKienItem> DanhSachSuKien { get; set; } = [];
}

/// <summary>Một dòng trong biểu đồ theo tháng.</summary>
public class BaoCaoTheoThangItem
{
    public int Nam { get; set; }
    public int Thang { get; set; }
    public string NhanThang => $"{Thang:D2}/{Nam}";
    public int SoLuong { get; set; }
}

/// <summary>Chi tiết một sự kiện trong khoảng thời gian báo cáo.</summary>
public class BaoCaoSuKienItem
{
    public string TenSuKien { get; set; } = string.Empty;
    public DateTime ThoiGianBatDau { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int SoDangKy { get; set; }
    public int SoCheckIn { get; set; }
}

