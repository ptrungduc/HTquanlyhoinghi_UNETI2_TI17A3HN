using HTquanlyhoinghi_UNETI2_TI17A3HN.Models;
using HTquanlyhoinghi_UNETI2_TI17A3HN.Models.Module4;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class ReceptionistDashboardViewModel
    {
        // Thông tin thống kê hôm nay
        public int TongSuKienHomNay { get; set; }
        public int TongDaCheckInHomNay { get; set; }
        public int TongChoCheckInHomNay { get; set; }
        public int TongDangKyHomNay { get; set; }

        // Danh sách sự kiện hôm nay / đang diễn ra / sắp tới
        public List<SuKienTiepDonViewModel> DanhSachSuKien { get; set; } = new();

        // Danh sách người tham dự để tra cứu & check-in nhanh
        public List<NguoiThamDuTiepDonViewModel> DanhSachNguoiThamDu { get; set; } = new();

        // Bộ lọc tra cứu
        public string? TuKhoa { get; set; }
        public string? MaSuKienChon { get; set; }

        // Kết quả thao tác nếu vừa thực hiện check-in nhanh
        public KetQuaThaoTacViewModel? KetQua { get; set; }
        public string? ThongBao { get; set; }
    }

    public class SuKienTiepDonViewModel
    {
        public string MaSuKien { get; set; } = string.Empty;
        public string TenSuKien { get; set; } = string.Empty;
        public string TenLoaiSuKien { get; set; } = string.Empty;
        public string TenDiaDiem { get; set; } = string.Empty;
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public int TongDangKy { get; set; }
        public int DaDuyet { get; set; }
        public int DaCheckIn { get; set; }
        public bool DangDienRa { get; set; }

        // Phiên của sự kiện này (để chuyển hướng điểm danh)
        public List<PhienTiepDonViewModel> DanhSachPhien { get; set; } = new();
    }

    public class PhienTiepDonViewModel
    {
        public string MaPhienSuKien { get; set; } = string.Empty;
        public string TenPhien { get; set; } = string.Empty;
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string PhongKhuVuc { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
    }

    public class NguoiThamDuTiepDonViewModel
    {
        public string MaDangKy { get; set; } = string.Empty;
        public string MaThamDu { get; set; } = string.Empty;
        public string MaNguoiThamDu { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DonViCongTac { get; set; } = string.Empty;
        public string MaSuKien { get; set; } = string.Empty;
        public string TenSuKien { get; set; } = string.Empty;
        public TrangThaiDangKy TrangThaiDangKy { get; set; }
        public DateTime? ThoiGianCheckIn { get; set; }
        public string? NguoiCheckIn { get; set; }
        public bool CoTheCheckIn { get; set; }
    }
}
