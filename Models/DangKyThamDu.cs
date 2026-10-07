// Entity dùng chung theo tài liệu yêu cầu (Mục 7.2 và Mục 10).
// Module 3 sử dụng cho nghiệp vụ đăng ký sự kiện.
// Module 4 sử dụng cho xét duyệt và check-in.
// Module 5 đọc dữ liệu cho lịch sử và thống kê.

using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class DangKyThamDu
    {
        [Key]
        public string MaDangKy { get; set; } = string.Empty;

        [Required]
        public string MaNguoiThamDu { get; set; } = string.Empty;

        [Required]
        public string MaSuKien { get; set; } = string.Empty;

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        // Trạng thái: ChoDuyet, DaDuyet, DaCheckIn, HoanThanh, TuChoi, DaHuy
        public TrangThaiDangKy TrangThai { get; set; } = TrangThaiDangKy.ChoDuyet;

        public string GhiChu { get; set; } = string.Empty;

        public DateTime? NgayDuyet { get; set; }

        public string LyDoTuChoiHuy { get; set; } = string.Empty;

        // Mã tham dự duy nhất, được cấp sau khi duyệt (Mục 8.2)
        [StringLength(100)]
        public string? MaThamDu { get; set; }

        // Navigation Property
        public NguoiThamDu? NguoiThamDu { get; set; }
        public SuKien? SuKien { get; set; }
        public CheckIn? CheckIn { get; set; }
    }
}
