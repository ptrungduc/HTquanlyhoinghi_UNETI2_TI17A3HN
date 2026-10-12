// Entity dùng chung theo tài liệu yêu cầu (Mục 8.3 và Mục 10).
// Module 4 sử dụng cho nghiệp vụ check-in.
// Module 5 đọc dữ liệu cho lịch sử và thống kê.

using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class CheckIn
    {
        [Key]
        public string MaCheckIn { get; set; } = string.Empty;

        [Required]
        public string MaDangKy { get; set; } = string.Empty;

        public DateTime ThoiGianCheckIn { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string NguoiThucHien { get; set; } = string.Empty;

        public string GhiChu { get; set; } = string.Empty;

        [StringLength(50)]
        public string TrangThai { get; set; } = "Thành công";

        // Navigation Property
        public DangKyThamDu? DangKyThamDu { get; set; }
    }
}
