// Entity dùng chung theo tài liệu yêu cầu (Mục 7.1 và Mục 10).
// Module 3 sử dụng cho nghiệp vụ đăng ký.
// Module 5 đọc dữ liệu cho lịch sử và thống kê.

using System.ComponentModel.DataAnnotations;

namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class NguoiThamDu
    {
        [Key]
        public string MaNguoiThamDu { get; set; } = string.Empty;

        [Required]
        public string MaTaiKhoan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0)")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string DonViCongTac { get; set; } = string.Empty;

        public bool TrangThai { get; set; } = true;

        // Navigation Property
        public TaiKhoan? TaiKhoan { get; set; }
        public ICollection<DangKyThamDu> DangKyThamDus { get; set; } = new List<DangKyThamDu>();
    }
}
