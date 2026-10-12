// Entity dùng chung theo tài liệu yêu cầu (Mục 7.6 và Mục 10).
// Module 3 sử dụng cho đăng ký phiên sự kiện.

using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class DangKyPhien
    {
        [Key]
        public string MaDangKyPhien { get; set; } = string.Empty;

        [Required]
        public string MaNguoiThamDu { get; set; } = string.Empty;

        [Required]
        public string MaPhien { get; set; } = string.Empty;

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        public TrangThaiDangKy TrangThai { get; set; } = TrangThaiDangKy.ChoDuyet;

        // Navigation Property
        public NguoiThamDu? NguoiThamDu { get; set; }
        public PhienSuKien? Phien { get; set; }
    }
}