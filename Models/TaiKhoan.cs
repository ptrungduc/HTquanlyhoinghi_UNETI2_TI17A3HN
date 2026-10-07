using System.ComponentModel.DataAnnotations;
namespace HTquanlyhoinghi_UNETI2_TI17A3HN.Models
{
    public class TaiKhoan
    {
        [Key]
        public string MaTaiKhoan { get; set; }
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        public string TenDangNhap { get; set; } = string.Empty;
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Required(ErrorMessage = "Email không được để trống")]
        public string Email { get; set; } = string.Empty;
        public string VaiTro { get; set; } = "Nguoi Tham Du";
        public bool TrangThai { get; set; } = true;
    }
}